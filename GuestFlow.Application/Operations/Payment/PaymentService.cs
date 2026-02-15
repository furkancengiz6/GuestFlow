using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Payment.Dtos;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IForeignKeyValidationService _foreignKeyValidationService;
        private readonly ICurrencyService _currencyService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly INotificationHubService _hubService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IRepository<PaymentEntity> paymentRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<GuestEntity> guestRepository,
            IForeignKeyValidationService foreignKeyValidationService,
            ICurrencyService currencyService,
            IMapper mapper,
            ILogger<PaymentService> logger,
            INotificationHubService? hubService = null)
        {
            _unitOfWork = unitOfWork;
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _guestRepository = guestRepository;
            _foreignKeyValidationService = foreignKeyValidationService;
            _currencyService = currencyService;
            _mapper = mapper;
            _logger = logger;
            _hubService = hubService;
        }

        public async Task<ServiceMessage<GetPaymentDto>> AddPaymentAsync(AddPaymentDto paymentDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Foreign key validasyonları - Guest ve Personnel zorunlu
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = paymentDto.GuestId,
                    PersonnelId = paymentDto.CollectedByPersonnelId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // Personel kontrolü - zorunlu (kim tahsil etti?)
                if (paymentDto.CollectedByPersonnelId <= 0)
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = "Ödemeyi tahsil eden personel belirtilmelidir." };
                }

                // Fatura kontrolü - opsiyonel
                if (paymentDto.InvoiceId.HasValue && paymentDto.InvoiceId.Value > 0)
                {
                    var invoice = await _invoiceRepository.GetAsync(x => x.Id == paymentDto.InvoiceId && !x.IsDeleted);
                    if (invoice == null)
                    {
                        return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = "Belirtilen fatura bulunamadı." };
                    }
                }

                // Servis bağlantısı kontrolü - en az biri olmalı veya genel ödeme olarak kaydedilmeli
                // Not: Hiçbiri belirtilmezse genel ödeme olarak kabul edilir (misafir kredisi gibi)

                // Para birimi validasyonu
                if (!_currencyService.IsValidCurrency(paymentDto.Currency))
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = "Geçersiz para birimi." };
                }

                // Ödeme yöntemi validasyonu
                if (!PaymentMethodHelper.IsValidMethod(paymentDto.PaymentMethod))
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = "Geçersiz ödeme yöntemi." };
                }

                // Tutar validasyonu
                if (paymentDto.Amount <= 0)
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = "Ödeme tutarı sıfırdan büyük olmalıdır." };
                }

                // Ödeme numarası oluştur
                var paymentNumber = await GeneratePaymentNumberAsync();

                // Ödeme entity oluştur
                var paymentEntity = new PaymentEntity
                {
                    PaymentNumber = paymentNumber,
                    InvoiceId = paymentDto.InvoiceId,
                    GuestId = paymentDto.GuestId,
                    CollectedByPersonnelId = paymentDto.CollectedByPersonnelId,
                    TransferId = paymentDto.TransferId,
                    CityTourId = paymentDto.CityTourId,
                    YachtTourId = paymentDto.YachtTourId,
                    Amount = paymentDto.Amount,
                    Currency = paymentDto.Currency,
                    PaymentMethod = PaymentMethodHelper.FromString(paymentDto.PaymentMethod),
                    Status = string.IsNullOrWhiteSpace(paymentDto.Status) ? PaymentStatus.Completed : PaymentStatusHelper.FromString(paymentDto.Status),
                    TransactionId = paymentDto.TransactionId,
                    PaymentDate = paymentDto.PaymentDate,
                    Notes = paymentDto.Notes
                };

                await _paymentRepository.AddAsync(paymentEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme/tahsilat kaydedildi: {paymentNumber}, Tutar: {paymentDto.Amount} {paymentDto.Currency}, Tahsil eden: {paymentDto.CollectedByPersonnelId}");

                // DTO'ya çevir ve döndür
                var paymentDtoResult = await GetPaymentByIdAsync(paymentEntity.Id);

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", paymentEntity.Id, "created");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                return new ServiceMessage<GetPaymentDto>
                {
                    IsSuccess = true,
                    Message = "Ödeme başarıyla kaydedildi.",
                    Data = paymentDtoResult
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = $"Ödeme oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdatePaymentAsync(UpdatePaymentDto paymentDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _paymentRepository.GetAsync(x => x.Id == paymentDto.Id && !x.IsDeleted);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme bulunamadı." };

                // Tamamlanmış veya iade edilmiş ödemeler güncellenemez
                if (existing.Status == PaymentStatus.Completed || existing.Status == PaymentStatus.Refunded)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu ödeme güncellenemez." };

                // Güncelleme
                if (paymentDto.Amount.HasValue)
                    existing.Amount = paymentDto.Amount.Value;

                if (!string.IsNullOrWhiteSpace(paymentDto.Currency))
                {
                    if (!_currencyService.IsValidCurrency(paymentDto.Currency))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçersiz para birimi." };
                    existing.Currency = paymentDto.Currency;
                }

                if (!string.IsNullOrWhiteSpace(paymentDto.PaymentMethod))
                {
                    if (!PaymentMethodHelper.IsValidMethod(paymentDto.PaymentMethod))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçersiz ödeme yöntemi." };
                    existing.PaymentMethod = PaymentMethodHelper.FromString(paymentDto.PaymentMethod);
                }

                if (!string.IsNullOrWhiteSpace(paymentDto.Status))
                {
                    if (!PaymentStatusHelper.IsValidStatus(paymentDto.Status))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçersiz ödeme durumu." };
                    existing.Status = PaymentStatusHelper.FromString(paymentDto.Status);
                }

                if (paymentDto.PaymentDate.HasValue)
                    existing.PaymentDate = paymentDto.PaymentDate.Value;

                if (!string.IsNullOrWhiteSpace(paymentDto.TransactionId))
                    existing.TransactionId = paymentDto.TransactionId;

                if (paymentDto.Notes != null)
                    existing.Notes = paymentDto.Notes;

                // Servis bağlantılarını güncelle (sonradan bağlama için)
                if (paymentDto.InvoiceId.HasValue)
                    existing.InvoiceId = paymentDto.InvoiceId;
                
                if (paymentDto.TransferId.HasValue)
                    existing.TransferId = paymentDto.TransferId;
                
                if (paymentDto.CityTourId.HasValue)
                    existing.CityTourId = paymentDto.CityTourId;
                
                if (paymentDto.YachtTourId.HasValue)
                    existing.YachtTourId = paymentDto.YachtTourId;

                await _paymentRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme güncellendi: {existing.PaymentNumber}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", existing.Id, "updated");
                }

                return new ServiceMessage { IsSuccess = true, Message = "Ödeme başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ödeme güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeletePaymentAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (payment == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme bulunamadı." };

                // Tamamlanmış ödemeler silinemez
                if (payment.Status == PaymentStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Tamamlanmış ödeme silinemez." };

                payment.IsDeleted = true;
                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme silindi: {payment.PaymentNumber}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", payment.Id, "deleted");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                return new ServiceMessage { IsSuccess = true, Message = "Ödeme başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ödeme silinirken hata: {ex.Message}" };
            }
        }

        public async Task<GetPaymentDto?> GetPaymentByIdAsync(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetAll()
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (payment == null)
                    return null;

                return _mapper.Map<GetPaymentDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödeme getirilirken hata: {ex.Message}");
                return null;
            }
        }

        public async Task<PaymentDetailDto?> GetPaymentDetailAsync(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetAll()
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (payment == null)
                    return null;

                return _mapper.Map<PaymentDetailDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödeme detayı getirilirken hata: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedResult<GetPaymentDto>> GetPaymentsPagedAsync(int pageNumber, int pageSize, PaymentFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _paymentRepository.GetAll(x => !x.IsDeleted)
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .AsQueryable();

                // Filtreleme
                query = query.ApplyPaymentFilters(filters);

                // Sıralama
                query = query.ApplyPaymentSorting(sorting);

                // Toplam kayıt sayısı
                var totalCount = await query.CountAsync();

                // Sayfalama
                var payments = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetPaymentDto>>(payments);

                return new PagedResult<GetPaymentDto>
                {
                    Data = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödemeler getirilirken hata: {ex.Message}");
                return new PagedResult<GetPaymentDto>
                {
                    Data = new List<GetPaymentDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        // Methods MapToGetPaymentDto and MapToPaymentDetailDto removed as they are replaced by AutoMapper.

        public async Task<ServiceMessage> CompletePaymentAsync(int paymentId, string transactionId, string? gatewayResponse = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetAsync(x => x.Id == paymentId && !x.IsDeleted);
                if (payment == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme bulunamadı." };

                if (payment.Status == PaymentStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme zaten tamamlanmış." };

                if (payment.Status == PaymentStatus.Cancelled || payment.Status == PaymentStatus.Refunded)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu ödeme tamamlanamaz." };

                payment.Status = PaymentStatus.Completed;
                payment.TransactionId = transactionId;
                payment.GatewayResponse = gatewayResponse;

                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme tamamlandı: {payment.PaymentNumber}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", payment.Id, "updated");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                return new ServiceMessage { IsSuccess = true, Message = "Ödeme başarıyla tamamlandı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme tamamlanırken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ödeme tamamlanırken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> FailPaymentAsync(int paymentId, string? reason = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetAsync(x => x.Id == paymentId && !x.IsDeleted);
                if (payment == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme bulunamadı." };

                if (payment.Status == PaymentStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Tamamlanmış ödeme başarısız olarak işaretlenemez." };

                payment.Status = PaymentStatus.Failed;
                if (!string.IsNullOrWhiteSpace(reason))
                    payment.Notes = $"Başarısız: {reason}";

                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme başarısız olarak işaretlendi: {payment.PaymentNumber}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", payment.Id, "updated");
                }

                return new ServiceMessage { IsSuccess = true, Message = "Ödeme başarısız olarak işaretlendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme başarısız işaretlenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ödeme başarısız işaretlenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> RefundPaymentAsync(int paymentId, string? refundReason = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetAsync(x => x.Id == paymentId && !x.IsDeleted);
                if (payment == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme bulunamadı." };

                if (payment.Status != PaymentStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Sadece tamamlanmış ödemeler iade edilebilir." };

                if (payment.Status == PaymentStatus.Refunded)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme zaten iade edilmiş." };

                payment.Status = PaymentStatus.Refunded;
                payment.RefundDate = DateTime.UtcNow;
                payment.RefundReason = refundReason;

                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme iade edildi: {payment.PaymentNumber}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", payment.Id, "updated");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                return new ServiceMessage { IsSuccess = true, Message = "Ödeme başarıyla iade edildi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme iade edilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ödeme iade edilirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> CancelPaymentAsync(int paymentId, string? cancellationReason = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetAsync(x => x.Id == paymentId && !x.IsDeleted);
                if (payment == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme bulunamadı." };

                if (payment.Status == PaymentStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Tamamlanmış ödeme iptal edilemez." };

                if (payment.Status == PaymentStatus.Refunded)
                    return new ServiceMessage { IsSuccess = false, Message = "İade edilmiş ödeme iptal edilemez." };

                if (payment.Status == PaymentStatus.Cancelled)
                    return new ServiceMessage { IsSuccess = false, Message = "Ödeme zaten iptal edilmiş." };

                payment.Status = PaymentStatus.Cancelled;
                payment.CancellationReason = cancellationReason;

                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme iptal edildi: {payment.PaymentNumber}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Payment", payment.Id, "updated");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                return new ServiceMessage { IsSuccess = true, Message = "Ödeme başarıyla iptal edildi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Ödeme iptal edilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ödeme iptal edilirken hata: {ex.Message}" };
            }
        }

        public async Task<List<GetPaymentDto>> GetPaymentsByGuestIdAsync(int guestId)
        {
            try
            {
                var payments = await _paymentRepository.GetAll(x => x.GuestId == guestId && !x.IsDeleted)
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return _mapper.Map<List<GetPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir ödemeleri getirilirken hata: {ex.Message}");
                return new List<GetPaymentDto>();
            }
        }

        public async Task<List<GetPaymentDto>> GetPaymentsByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                var payments = await _paymentRepository.GetAll(x => x.InvoiceId == invoiceId && !x.IsDeleted)
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return _mapper.Map<List<GetPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura ödemeleri getirilirken hata: {ex.Message}");
                return new List<GetPaymentDto>();
            }
        }

        public async Task<List<GetPaymentDto>> GetPaymentsByStatusAsync(string status)
        {
            try
            {
                if (!PaymentStatusHelper.IsValidStatus(status))
                    return new List<GetPaymentDto>();

                var paymentStatus = PaymentStatusHelper.FromString(status);
                var payments = await _paymentRepository.GetAll(x => x.Status == paymentStatus && !x.IsDeleted)
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return _mapper.Map<List<GetPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Durum bazlı ödemeler getirilirken hata: {ex.Message}");
                return new List<GetPaymentDto>();
            }
        }

        /// <summary>
        /// Belirli bir servis için toplam ödeme tutarını hesaplar
        /// </summary>
        public async Task<decimal> GetTotalPaidForTransferAsync(int transferId)
        {
            return await _paymentRepository.GetAll(x => x.TransferId == transferId && x.Status == PaymentStatus.Completed && !x.IsDeleted)
                .SumAsync(p => p.Amount);
        }

        /// <summary>
        /// Belirli bir şehir turu için toplam ödeme tutarını hesaplar
        /// </summary>
        public async Task<decimal> GetTotalPaidForCityTourAsync(int cityTourId)
        {
            return await _paymentRepository.GetAll(x => x.CityTourId == cityTourId && x.Status == PaymentStatus.Completed && !x.IsDeleted)
                .SumAsync(p => p.Amount);
        }

        /// <summary>
        /// Belirli bir yat turu için toplam ödeme tutarını hesaplar
        /// </summary>
        public async Task<decimal> GetTotalPaidForYachtTourAsync(int yachtTourId)
        {
            return await _paymentRepository.GetAll(x => x.YachtTourId == yachtTourId && x.Status == PaymentStatus.Completed && !x.IsDeleted)
                .SumAsync(p => p.Amount);
        }

        /// <summary>
        /// Belirli bir tarih aralığında toplanan gelirleri currency bazlı hesaplar
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _paymentRepository.GetAll(
                x => x.PaymentDate.Date >= startDate.Date && 
                     x.PaymentDate.Date <= endDate.Date && 
                     x.Status == PaymentStatus.Completed && 
                     !x.IsDeleted)
                .GroupBy(p => p.Currency)
                .Select(g => new { Currency = g.Key, Total = g.Sum(p => p.Amount) })
                .ToListAsync();

            return payments.ToDictionary(x => x.Currency, x => x.Total);
        }

        /// <summary>
        /// Belirli bir gün için toplanan gelirleri currency bazlı hesaplar
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetDailyRevenueAsync(DateTime date)
        {
            return await GetRevenueByDateRangeAsync(date, date);
        }

        public async Task<string> GeneratePaymentNumberAsync()
        {
            try
            {
                string paymentNumber;
                bool isUnique = false;
                int attempts = 0;
                const int maxAttempts = 10;

                do
                {
                    // Format: PAY-YYYYMMDD-HHMMSS-XXXX (XXXX = random 4 digit)
                    var now = DateTime.UtcNow;
                    var random = new Random();
                    var randomPart = random.Next(1000, 9999);
                    paymentNumber = $"PAY-{now:yyyyMMdd}-{now:HHmmss}-{randomPart}";

                    var exists = await _paymentRepository.GetAll(x => x.PaymentNumber == paymentNumber && !x.IsDeleted).AnyAsync();
                    isUnique = !exists;
                    attempts++;

                    if (attempts >= maxAttempts)
                    {
                        // Fallback: timestamp + GUID kısa versiyonu
                        paymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                        break;
                    }
                } while (!isUnique);

                return paymentNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödeme numarası oluşturulurken hata: {ex.Message}");
                // Fallback
                return $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
            }
        }
        public async Task<GetPaymentDto?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                    return null;

                var payment = await _paymentRepository.GetAll()
                    .Include(p => p.Invoice)
                    .Include(p => p.Guest)
                    .Include(p => p.CollectedByPersonnel)
                    .Include(p => p.Transfer)
                    .Include(p => p.CityTour)
                    .Include(p => p.YachtTour)
                    .FirstOrDefaultAsync(x => x.TransactionId == transactionId && !x.IsDeleted);

                if (payment == null)
                    return null;

                return _mapper.Map<GetPaymentDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transaction ID bazlı ödeme getirilirken hata: {ex.Message}");
                return null;
            }
        }
    }
}

