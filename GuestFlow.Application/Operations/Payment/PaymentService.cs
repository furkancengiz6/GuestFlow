using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Currency;
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

        public PaymentService(
            IUnitOfWork unitOfWork,
            IRepository<PaymentEntity> paymentRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<GuestEntity> guestRepository,
            IForeignKeyValidationService foreignKeyValidationService,
            ICurrencyService currencyService,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _guestRepository = guestRepository;
            _foreignKeyValidationService = foreignKeyValidationService;
            _currencyService = currencyService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceMessage<GetPaymentDto>> AddPaymentAsync(AddPaymentDto paymentDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Foreign key validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = paymentDto.GuestId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // Fatura kontrolü
                var invoice = await _invoiceRepository.GetAsync(x => x.Id == paymentDto.InvoiceId && !x.IsDeleted);
                if (invoice == null)
                {
                    return new ServiceMessage<GetPaymentDto> { IsSuccess = false, Message = "Fatura bulunamadı." };
                }

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

                // Ödeme numarası oluştur
                var paymentNumber = await GeneratePaymentNumberAsync();

                // Ödeme entity oluştur
                var paymentEntity = _mapper.Map<PaymentEntity>(paymentDto);
                paymentEntity.PaymentNumber = paymentNumber;
                paymentEntity.PaymentMethod = PaymentMethodHelper.FromString(paymentDto.PaymentMethod);
                paymentEntity.Status = PaymentStatus.Pending;

                await _paymentRepository.AddAsync(paymentEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme oluşturuldu: {paymentNumber}");

                // DTO'ya çevir ve döndür
                var paymentDtoResult = await GetPaymentByIdAsync(paymentEntity.Id);
                return new ServiceMessage<GetPaymentDto>
                {
                    IsSuccess = true,
                    Message = "Ödeme başarıyla oluşturuldu.",
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

                await _paymentRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Ödeme güncellendi: {existing.PaymentNumber}");
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
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (payment == null)
                    return null;

                var dto = _mapper.Map<GetPaymentDto>(payment);
                dto.InvoiceNumber = payment.Invoice?.InvoiceNumber ?? 0;
                dto.GuestName = payment.Guest?.FullName ?? string.Empty;

                return dto;
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
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (payment == null)
                    return null;

                var dto = _mapper.Map<PaymentDetailDto>(payment);
                dto.InvoiceNumber = payment.Invoice?.InvoiceNumber ?? 0;
                dto.InvoiceAmount = payment.Invoice?.TotalAmount ?? 0;
                dto.InvoiceCurrency = payment.Invoice?.Currency ?? string.Empty;
                dto.GuestName = payment.Guest?.FullName ?? string.Empty;
                dto.GuestEmail = payment.Guest?.Email ?? string.Empty;
                dto.GuestPhoneNumber = payment.Guest?.PhoneNumber ?? string.Empty;
                dto.Status = PaymentStatusHelper.ToString(payment.Status);
                dto.PaymentMethod = PaymentMethodHelper.ToString(payment.PaymentMethod);

                return dto;
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

                var dtos = payments.Select(p => new GetPaymentDto
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    InvoiceId = p.InvoiceId,
                    InvoiceNumber = p.Invoice?.InvoiceNumber ?? 0,
                    GuestId = p.GuestId,
                    GuestName = p.Guest?.FullName ?? string.Empty,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = PaymentMethodHelper.ToString(p.PaymentMethod),
                    Status = PaymentStatusHelper.ToString(p.Status),
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    RefundDate = p.RefundDate,
                    RefundReason = p.RefundReason,
                    CreatedDate = p.CreatedDate
                }).ToList();

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
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(p => new GetPaymentDto
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    InvoiceId = p.InvoiceId,
                    InvoiceNumber = p.Invoice?.InvoiceNumber ?? 0,
                    GuestId = p.GuestId,
                    GuestName = p.Guest?.FullName ?? string.Empty,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = PaymentMethodHelper.ToString(p.PaymentMethod),
                    Status = PaymentStatusHelper.ToString(p.Status),
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    RefundDate = p.RefundDate,
                    RefundReason = p.RefundReason,
                    CreatedDate = p.CreatedDate
                }).ToList();
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
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(p => new GetPaymentDto
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    InvoiceId = p.InvoiceId,
                    InvoiceNumber = p.Invoice?.InvoiceNumber ?? 0,
                    GuestId = p.GuestId,
                    GuestName = p.Guest?.FullName ?? string.Empty,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = PaymentMethodHelper.ToString(p.PaymentMethod),
                    Status = PaymentStatusHelper.ToString(p.Status),
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    RefundDate = p.RefundDate,
                    RefundReason = p.RefundReason,
                    CreatedDate = p.CreatedDate
                }).ToList();
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
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(p => new GetPaymentDto
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    InvoiceId = p.InvoiceId,
                    InvoiceNumber = p.Invoice?.InvoiceNumber ?? 0,
                    GuestId = p.GuestId,
                    GuestName = p.Guest?.FullName ?? string.Empty,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = PaymentMethodHelper.ToString(p.PaymentMethod),
                    Status = PaymentStatusHelper.ToString(p.Status),
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    RefundDate = p.RefundDate,
                    RefundReason = p.RefundReason,
                    CreatedDate = p.CreatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Durum bazlı ödemeler getirilirken hata: {ex.Message}");
                return new List<GetPaymentDto>();
            }
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
    }
}

