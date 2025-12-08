using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Sms.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Sms
{
    public class SmsService : ISmsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SmsHistoryEntity> _smsHistoryRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<ReservationEntity> _reservationRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<SmsService> _logger;

        private readonly bool _smsEnabled;
        private readonly string _smsProvider;
        private readonly string _smsApiKey;
        private readonly string _smsApiSecret;
        private readonly string _smsSenderName;

        public SmsService(
            IUnitOfWork unitOfWork,
            IRepository<SmsHistoryEntity> smsHistoryRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<ReservationEntity> reservationRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<SmsService> logger)
        {
            _unitOfWork = unitOfWork;
            _smsHistoryRepository = smsHistoryRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _reservationRepository = reservationRepository;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;

            _smsEnabled = bool.Parse(_configuration["SmsSettings:Enabled"] ?? "false");
            _smsProvider = _configuration["SmsSettings:Provider"] ?? "Mock";
            _smsApiKey = _configuration["SmsSettings:ApiKey"] ?? string.Empty;
            _smsApiSecret = _configuration["SmsSettings:ApiSecret"] ?? string.Empty;
            _smsSenderName = _configuration["SmsSettings:SenderName"] ?? "GuestFlow";
        }

        public async Task<ServiceMessage<GetSmsHistoryDto>> SendSmsAsync(SendSmsDto smsDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Telefon numarası validasyonu
                if (!IsValidPhoneNumber(smsDto.PhoneNumber))
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Geçersiz telefon numarası formatı."
                    };
                }

                // SMS geçmişi kaydı oluştur
                var smsHistory = new SmsHistoryEntity
                {
                    PhoneNumber = NormalizePhoneNumber(smsDto.PhoneNumber),
                    Message = smsDto.Message,
                    Status = SmsStatus.Pending,
                    SentDate = DateTime.UtcNow,
                    GuestId = smsDto.GuestId,
                    PersonnelId = smsDto.PersonnelId,
                    RelatedEntityType = smsDto.RelatedEntityType,
                    RelatedEntityId = smsDto.RelatedEntityId,
                    SmsType = smsDto.SmsType,
                    TemplateName = smsDto.TemplateName,
                    Provider = _smsProvider
                };

                await _smsHistoryRepository.AddAsync(smsHistory);
                await _unitOfWork.SaveChangesAsync();

                // SMS gönderimi
                bool sendResult = false;
                string? messageId = null;
                string? gatewayResponse = null;
                string? errorMessage = null;

                if (_smsEnabled)
                {
                    try
                    {
                        // Gerçek SMS gateway entegrasyonu burada yapılacak
                        // Şu an mock implementasyon
                        sendResult = await SendSmsToGatewayAsync(smsHistory.PhoneNumber, smsHistory.Message);
                        
                        if (sendResult)
                        {
                            messageId = Guid.NewGuid().ToString();
                            gatewayResponse = "{\"status\":\"success\",\"messageId\":\"" + messageId + "\"}";
                            smsHistory.Status = SmsStatus.Sent;
                            smsHistory.MessageId = messageId;
                            smsHistory.GatewayResponse = gatewayResponse;
                        }
                        else
                        {
                            smsHistory.Status = SmsStatus.Failed;
                            errorMessage = "SMS gateway'den hata alındı.";
                            smsHistory.ErrorMessage = errorMessage;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"SMS gönderilirken hata: {ex.Message}");
                        smsHistory.Status = SmsStatus.Failed;
                        errorMessage = ex.Message;
                        smsHistory.ErrorMessage = errorMessage;
                    }
                }
                else
                {
                    _logger.LogInformation($"SMS servisi devre dışı. SMS gönderilmedi: {smsHistory.PhoneNumber}");
                    smsHistory.Status = SmsStatus.Failed;
                    errorMessage = "SMS servisi devre dışı.";
                    smsHistory.ErrorMessage = errorMessage;
                }

                await _smsHistoryRepository.UpdateAsync(smsHistory);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var dto = _mapper.Map<GetSmsHistoryDto>(smsHistory);
                if (smsHistory.Guest != null)
                    dto.GuestName = smsHistory.Guest.FullName;
                if (smsHistory.Personnel != null)
                    dto.PersonnelName = smsHistory.Personnel.FullName;

                if (sendResult)
                {
                    _logger.LogInformation($"SMS başarıyla gönderildi: {smsHistory.PhoneNumber}, MessageId: {messageId}");
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = true,
                        Message = "SMS başarıyla gönderildi.",
                        Data = dto
                    };
                }
                else
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = errorMessage ?? "SMS gönderilemedi.",
                        Data = dto
                    };
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"SMS gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetSmsHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"SMS gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GetSmsHistoryDto>> SendTransferReminderAsync(int transferId, int hoursBefore = 24)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .FirstOrDefaultAsync(t => t.Id == transferId && !t.IsDeleted);

                if (transfer == null)
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Transfer bulunamadı."
                    };
                }

                if (transfer.Guest == null || string.IsNullOrEmpty(transfer.Guest.PhoneNumber))
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Misafir telefon numarası bulunamadı."
                    };
                }

                var transferDate = transfer.TransferDate;
                var reminderTime = transferDate.AddHours(-hoursBefore);

                // Hatırlatma zamanı geçmişse gönderme
                if (reminderTime < DateTime.UtcNow)
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Hatırlatma zamanı geçmiş."
                    };
                }

                var message = $"Sayın {transfer.Guest.FullName}, {transferDate:dd.MM.yyyy HH:mm} tarihinde transferiniz var. " +
                             $"Kalkış: {transfer.PickupAddress}, Varış: {transfer.DropoffAddress}. " +
                             $"GuestFlow";

                var smsDto = new SendSmsDto
                {
                    PhoneNumber = transfer.Guest.PhoneNumber,
                    Message = message,
                    GuestId = transfer.GuestId,
                    RelatedEntityType = "Transfer",
                    RelatedEntityId = transferId,
                    SmsType = "Reminder"
                };

                return await SendSmsAsync(smsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer hatırlatma SMS'i gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetSmsHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"Transfer hatırlatma SMS'i gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GetSmsHistoryDto>> SendTourReminderAsync(string tourType, int tourId, int hoursBefore = 24)
        {
            try
            {
                GuestEntity? guest = null;
                DateTime tourDate;
                string tourName = string.Empty;
                string location = string.Empty;

                if (tourType.ToLower() == "citytour")
                {
                    var cityTour = await _cityTourRepository.GetAll()
                        .Include(t => t.OwnerGuest)
                        .Include(t => t.City)
                        .FirstOrDefaultAsync(t => t.Id == tourId && !t.IsDeleted);

                    if (cityTour == null)
                    {
                        return new ServiceMessage<GetSmsHistoryDto>
                        {
                            IsSuccess = false,
                            Message = "Şehir turu bulunamadı."
                        };
                    }

                    guest = cityTour.OwnerGuest;
                    tourDate = cityTour.TourDate;
                    tourName = "Şehir Turu";
                    location = cityTour.City?.CityName ?? "Bilinmiyor";
                }
                else if (tourType.ToLower() == "yachttour")
                {
                    var yachtTour = await _yachtTourRepository.GetAll()
                        .Include(t => t.OwnerGuest)
                        .Include(t => t.City)
                        .FirstOrDefaultAsync(t => t.Id == tourId && !t.IsDeleted);

                    if (yachtTour == null)
                    {
                        return new ServiceMessage<GetSmsHistoryDto>
                        {
                            IsSuccess = false,
                            Message = "Yat turu bulunamadı."
                        };
                    }

                    guest = yachtTour.OwnerGuest;
                    tourDate = yachtTour.TourDate;
                    tourName = "Yat Turu";
                    location = yachtTour.City?.CityName ?? "Bilinmiyor";
                }
                else
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Geçersiz tur tipi."
                    };
                }

                if (guest == null || string.IsNullOrEmpty(guest.PhoneNumber))
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Misafir telefon numarası bulunamadı."
                    };
                }

                var reminderTime = tourDate.AddHours(-hoursBefore);
                if (reminderTime < DateTime.UtcNow)
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Hatırlatma zamanı geçmiş."
                    };
                }

                var message = $"Sayın {guest.FullName}, {tourDate:dd.MM.yyyy HH:mm} tarihinde {tourName} rezervasyonunuz var. " +
                             $"Lokasyon: {location}. GuestFlow";

                var smsDto = new SendSmsDto
                {
                    PhoneNumber = guest.PhoneNumber,
                    Message = message,
                    GuestId = guest.Id,
                    RelatedEntityType = tourType,
                    RelatedEntityId = tourId,
                    SmsType = "Reminder"
                };

                return await SendSmsAsync(smsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur hatırlatma SMS'i gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetSmsHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"Tur hatırlatma SMS'i gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GetSmsHistoryDto>> SendReservationConfirmationAsync(int reservationId)
        {
            try
            {
                var reservation = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Rezervasyon bulunamadı."
                    };
                }

                if (reservation.Guest == null || string.IsNullOrEmpty(reservation.Guest.PhoneNumber))
                {
                    return new ServiceMessage<GetSmsHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Misafir telefon numarası bulunamadı."
                    };
                }

                var message = $"Sayın {reservation.Guest.FullName}, rezervasyonunuz onaylandı. " +
                             $"Rezervasyon No: {reservation.ReservationNumber}, " +
                             $"Tarih: {reservation.ReservationDate:dd.MM.yyyy HH:mm}, " +
                             $"Tutar: {reservation.TotalAmount} {reservation.Currency}. GuestFlow";

                var smsDto = new SendSmsDto
                {
                    PhoneNumber = reservation.Guest.PhoneNumber,
                    Message = message,
                    GuestId = reservation.GuestId,
                    RelatedEntityType = "Reservation",
                    RelatedEntityId = reservationId,
                    SmsType = "Confirmation"
                };

                return await SendSmsAsync(smsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon onay SMS'i gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetSmsHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"Rezervasyon onay SMS'i gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<GetSmsHistoryDto?> GetSmsHistoryByIdAsync(int id)
        {
            try
            {
                var sms = await _smsHistoryRepository.GetAll()
                    .Include(s => s.Guest)
                    .Include(s => s.Personnel)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (sms == null)
                    return null;

                var dto = _mapper.Map<GetSmsHistoryDto>(sms);
                dto.GuestName = sms.Guest?.FullName;
                dto.PersonnelName = sms.Personnel?.FullName;
                dto.Status = SmsStatusHelper.ToString(sms.Status);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMS geçmişi getirilirken hata: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedResult<GetSmsHistoryDto>> GetSmsHistoryPagedAsync(int pageNumber, int pageSize, SmsFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _smsHistoryRepository.GetAll(x => !x.IsDeleted)
                    .Include(s => s.Guest)
                    .Include(s => s.Personnel)
                    .AsQueryable();

                // Filtreleme
                query = query.ApplySmsFilters(filters);

                // Sıralama
                query = query.ApplySmsSorting(sorting);

                // Toplam kayıt sayısı
                var totalCount = await query.CountAsync();

                // Sayfalama
                var smsList = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = smsList.Select(s => new GetSmsHistoryDto
                {
                    Id = s.Id,
                    PhoneNumber = s.PhoneNumber,
                    Message = s.Message,
                    Status = SmsStatusHelper.ToString(s.Status),
                    SentDate = s.SentDate,
                    DeliveredDate = s.DeliveredDate,
                    ErrorMessage = s.ErrorMessage,
                    Provider = s.Provider,
                    MessageId = s.MessageId,
                    TemplateName = s.TemplateName,
                    RelatedEntityType = s.RelatedEntityType,
                    RelatedEntityId = s.RelatedEntityId,
                    GuestId = s.GuestId,
                    GuestName = s.Guest?.FullName,
                    PersonnelId = s.PersonnelId,
                    PersonnelName = s.Personnel?.FullName,
                    SmsType = s.SmsType,
                    CreatedDate = s.CreatedDate
                }).ToList();

                return new PagedResult<GetSmsHistoryDto>
                {
                    Data = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMS geçmişi getirilirken hata: {ex.Message}");
                return new PagedResult<GetSmsHistoryDto>
                {
                    Data = new List<GetSmsHistoryDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public async Task<List<GetSmsHistoryDto>> GetSmsHistoryByGuestIdAsync(int guestId)
        {
            try
            {
                var smsList = await _smsHistoryRepository.GetAll(x => x.GuestId == guestId && !x.IsDeleted)
                    .Include(s => s.Guest)
                    .Include(s => s.Personnel)
                    .OrderByDescending(s => s.SentDate)
                    .ToListAsync();

                return smsList.Select(s => new GetSmsHistoryDto
                {
                    Id = s.Id,
                    PhoneNumber = s.PhoneNumber,
                    Message = s.Message,
                    Status = SmsStatusHelper.ToString(s.Status),
                    SentDate = s.SentDate,
                    DeliveredDate = s.DeliveredDate,
                    ErrorMessage = s.ErrorMessage,
                    Provider = s.Provider,
                    MessageId = s.MessageId,
                    TemplateName = s.TemplateName,
                    RelatedEntityType = s.RelatedEntityType,
                    RelatedEntityId = s.RelatedEntityId,
                    GuestId = s.GuestId,
                    GuestName = s.Guest?.FullName,
                    PersonnelId = s.PersonnelId,
                    PersonnelName = s.Personnel?.FullName,
                    SmsType = s.SmsType,
                    CreatedDate = s.CreatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir SMS geçmişi getirilirken hata: {ex.Message}");
                return new List<GetSmsHistoryDto>();
            }
        }

        public async Task<List<GetSmsHistoryDto>> GetSmsHistoryByStatusAsync(string status)
        {
            try
            {
                if (!SmsStatusHelper.IsValidStatus(status))
                    return new List<GetSmsHistoryDto>();

                var smsStatus = SmsStatusHelper.FromString(status);
                var smsList = await _smsHistoryRepository.GetAll(x => x.Status == smsStatus && !x.IsDeleted)
                    .Include(s => s.Guest)
                    .Include(s => s.Personnel)
                    .OrderByDescending(s => s.SentDate)
                    .ToListAsync();

                return smsList.Select(s => new GetSmsHistoryDto
                {
                    Id = s.Id,
                    PhoneNumber = s.PhoneNumber,
                    Message = s.Message,
                    Status = SmsStatusHelper.ToString(s.Status),
                    SentDate = s.SentDate,
                    DeliveredDate = s.DeliveredDate,
                    ErrorMessage = s.ErrorMessage,
                    Provider = s.Provider,
                    MessageId = s.MessageId,
                    TemplateName = s.TemplateName,
                    RelatedEntityType = s.RelatedEntityType,
                    RelatedEntityId = s.RelatedEntityId,
                    GuestId = s.GuestId,
                    GuestName = s.Guest?.FullName,
                    PersonnelId = s.PersonnelId,
                    PersonnelName = s.Personnel?.FullName,
                    SmsType = s.SmsType,
                    CreatedDate = s.CreatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Durum bazlı SMS geçmişi getirilirken hata: {ex.Message}");
                return new List<GetSmsHistoryDto>();
            }
        }

        public async Task<ServiceMessage> UpdateSmsStatusAsync(int smsId, string status, string? messageId = null, string? gatewayResponse = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var sms = await _smsHistoryRepository.GetAsync(x => x.Id == smsId && !x.IsDeleted);
                if (sms == null)
                    return new ServiceMessage { IsSuccess = false, Message = "SMS kaydı bulunamadı." };

                if (!SmsStatusHelper.IsValidStatus(status))
                    return new ServiceMessage { IsSuccess = false, Message = "Geçersiz SMS durumu." };

                sms.Status = SmsStatusHelper.FromString(status);
                if (!string.IsNullOrEmpty(messageId))
                    sms.MessageId = messageId;
                if (!string.IsNullOrEmpty(gatewayResponse))
                    sms.GatewayResponse = gatewayResponse;

                if (sms.Status == SmsStatus.Delivered && sms.DeliveredDate == null)
                    sms.DeliveredDate = DateTime.UtcNow;

                await _smsHistoryRepository.UpdateAsync(sms);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"SMS durumu güncellendi: {smsId}, Status: {status}");
                return new ServiceMessage { IsSuccess = true, Message = "SMS durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"SMS durumu güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"SMS durumu güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<SmsStatisticsDto> GetSmsStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _smsHistoryRepository.GetAll(x => !x.IsDeleted).AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(s => s.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(s => s.SentDate <= endDate.Value);

                var smsList = await query.ToListAsync();

                var statistics = new SmsStatisticsDto
                {
                    TotalSent = smsList.Count,
                    TotalDelivered = smsList.Count(s => s.Status == SmsStatus.Delivered),
                    TotalFailed = smsList.Count(s => s.Status == SmsStatus.Failed),
                    TotalPending = smsList.Count(s => s.Status == SmsStatus.Pending)
                };

                if (statistics.TotalSent > 0)
                {
                    statistics.SuccessRate = (decimal)(statistics.TotalDelivered + statistics.TotalSent - statistics.TotalFailed) / statistics.TotalSent * 100;
                }

                // SMS tipine göre gruplama
                statistics.SmsByType = smsList
                    .Where(s => !string.IsNullOrEmpty(s.SmsType))
                    .GroupBy(s => s.SmsType)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Duruma göre gruplama
                statistics.SmsByStatus = smsList
                    .GroupBy(s => SmsStatusHelper.ToString(s.Status))
                    .ToDictionary(g => g.Key, g => g.Count());

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMS istatistikleri getirilirken hata: {ex.Message}");
                return new SmsStatisticsDto();
            }
        }

        #region Private Methods

        /// <summary>
        /// Telefon numarası validasyonu
        /// </summary>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Türkiye telefon numarası formatı: +90XXXXXXXXXX veya 0XXXXXXXXXX
            var pattern = @"^(\+90|0)?[5][0-9]{9}$";
            return Regex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", ""), pattern);
        }

        /// <summary>
        /// Telefon numarasını normalize eder (+90XXXXXXXXXX formatına)
        /// </summary>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var normalized = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (normalized.StartsWith("+90"))
                return normalized;
            else if (normalized.StartsWith("0"))
                return "+90" + normalized.Substring(1);
            else if (normalized.StartsWith("90"))
                return "+" + normalized;
            else if (normalized.StartsWith("5"))
                return "+90" + normalized;
            else
                return normalized;
        }

        /// <summary>
        /// SMS gateway'e gönderir (mock implementasyon - gerçek gateway entegrasyonu için güncellenebilir)
        /// </summary>
        private async Task<bool> SendSmsToGatewayAsync(string phoneNumber, string message)
        {
            // Mock implementasyon - gerçek SMS gateway entegrasyonu burada yapılacak
            // Örnek: Netgsm, Twilio, IletiMerkezi, vb.
            
            await Task.Delay(100); // Simüle edilmiş API çağrısı

            // Gerçek implementasyon örneği:
            /*
            if (_smsProvider == "Netgsm")
            {
                // Netgsm API entegrasyonu
                var client = new HttpClient();
                var response = await client.PostAsync("https://api.netgsm.com.tr/sms/send/get", ...);
                return response.IsSuccessStatusCode;
            }
            else if (_smsProvider == "Twilio")
            {
                // Twilio API entegrasyonu
                // ...
            }
            */

            // Mock: Her zaman başarılı döndür
            return true;
        }

        #endregion
    }
}

