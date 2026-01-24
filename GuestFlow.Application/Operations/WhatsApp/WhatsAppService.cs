// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.WhatsApp.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.WhatsApp
{
    /// <summary>
    /// WhatsApp servisi implementasyonu
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WhatsAppHistoryEntity> _whatsAppHistoryRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<ReservationEntity> _reservationRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<WhatsAppService> _logger;

        private readonly bool _whatsAppEnabled;
        private readonly string _whatsAppProvider;
        private readonly string _whatsAppApiKey;
        private readonly string _whatsAppApiSecret;
        private readonly string _whatsAppPhoneNumberId;
        private readonly string _whatsAppBusinessAccountId;
        private readonly string _whatsAppAccessToken;
        private readonly string _whatsAppWebhookVerifyToken;

        public WhatsAppService(
            IUnitOfWork unitOfWork,
            IRepository<WhatsAppHistoryEntity> whatsAppHistoryRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<ReservationEntity> reservationRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<WhatsAppService> logger)
        {
            _unitOfWork = unitOfWork;
            _whatsAppHistoryRepository = whatsAppHistoryRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _reservationRepository = reservationRepository;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;

            _whatsAppEnabled = bool.Parse(_configuration["WhatsAppSettings:Enabled"] ?? "false");
            _whatsAppProvider = _configuration["WhatsAppSettings:Provider"] ?? "Meta";
            _whatsAppApiKey = _configuration["WhatsAppSettings:ApiKey"] ?? string.Empty;
            _whatsAppApiSecret = _configuration["WhatsAppSettings:ApiSecret"] ?? string.Empty;
            _whatsAppPhoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"] ?? string.Empty;
            _whatsAppBusinessAccountId = _configuration["WhatsAppSettings:BusinessAccountId"] ?? string.Empty;
            _whatsAppAccessToken = _configuration["WhatsAppSettings:AccessToken"] ?? string.Empty;
            _whatsAppWebhookVerifyToken = _configuration["WhatsAppSettings:WebhookVerifyToken"] ?? string.Empty;
        }

        public async Task<ServiceMessage<GetWhatsAppHistoryDto>> SendWhatsAppAsync(SendWhatsAppDto whatsAppDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Telefon numarası validasyonu ve normalizasyonu
                if (!IsValidPhoneNumber(whatsAppDto.PhoneNumber))
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Geçersiz telefon numarası formatı."
                    };
                }

                var normalizedPhone = NormalizePhoneNumber(whatsAppDto.PhoneNumber);

                // WhatsApp geçmişi kaydı oluştur
                var whatsAppHistory = new WhatsAppHistoryEntity
                {
                    PhoneNumber = normalizedPhone,
                    Message = whatsAppDto.Message,
                    Status = "Pending",
                    SentDate = DateTime.UtcNow,
                    GuestId = whatsAppDto.GuestId,
                    PersonnelId = whatsAppDto.PersonnelId,
                    RelatedEntityType = whatsAppDto.RelatedEntityType,
                    RelatedEntityId = whatsAppDto.RelatedEntityId,
                    MessageCategory = whatsAppDto.TemplateName,
                    MessageType = whatsAppDto.MessageType.ToString(),
                    Provider = _whatsAppProvider,
                    TemplateName = whatsAppDto.TemplateName
                };

                // Template parametrelerini JSON olarak kaydet
                if (whatsAppDto.TemplateParameters != null && whatsAppDto.TemplateParameters.Any())
                {
                    whatsAppHistory.TemplateParameters = JsonSerializer.Serialize(whatsAppDto.TemplateParameters);
                }

                // Rich message data'yı JSON olarak kaydet
                if (whatsAppDto.RichMessage != null)
                {
                    whatsAppHistory.RichMessageData = JsonSerializer.Serialize(whatsAppDto.RichMessage);
                }

                await _whatsAppHistoryRepository.AddAsync(whatsAppHistory);
                await _unitOfWork.SaveChangesAsync();

                // WhatsApp mesajını gönder
                bool sendResult = false;
                string? messageId = null;
                string? gatewayResponse = null;
                string? errorMessage = null;

                if (_whatsAppEnabled)
                {
                    try
                    {
                        // WhatsApp Business API'ye mesaj gönder
                        var result = await SendWhatsAppToGatewayAsync(whatsAppDto, normalizedPhone);
                        sendResult = result.Success;
                        messageId = result.MessageId;
                        gatewayResponse = result.GatewayResponse;
                        
                        if (sendResult)
                        {
                            whatsAppHistory.Status = "Sent";
                            whatsAppHistory.MessageId = messageId;
                            whatsAppHistory.GatewayResponse = gatewayResponse;
                        }
                        else
                        {
                            whatsAppHistory.Status = "Failed";
                            errorMessage = result.ErrorMessage ?? "WhatsApp gateway'den hata alındı.";
                            whatsAppHistory.ErrorMessage = errorMessage;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"WhatsApp gönderilirken hata: {ex.Message}");
                        whatsAppHistory.Status = "Failed";
                        errorMessage = ex.Message;
                        whatsAppHistory.ErrorMessage = errorMessage;
                    }
                }
                else
                {
                    _logger.LogInformation($"WhatsApp servisi devre dışı. Mesaj gönderilmedi: {normalizedPhone}");
                    whatsAppHistory.Status = "Failed";
                    errorMessage = "WhatsApp servisi devre dışı.";
                    whatsAppHistory.ErrorMessage = errorMessage;
                }

                await _whatsAppHistoryRepository.UpdateAsync(whatsAppHistory);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Include related entities for mapping
                if (whatsAppHistory.GuestId.HasValue)
                {
                    whatsAppHistory.Guest = await _guestRepository.GetAll()
                        .FirstOrDefaultAsync(g => g.Id == whatsAppHistory.GuestId.Value);
                }

                if (whatsAppHistory.PersonnelId.HasValue)
                {
                    whatsAppHistory.Personnel = await _personnelRepository.GetAll()
                        .FirstOrDefaultAsync(p => p.Id == whatsAppHistory.PersonnelId.Value);
                }

                var dto = _mapper.Map<GetWhatsAppHistoryDto>(whatsAppHistory);

                if (sendResult)
                {
                    _logger.LogInformation($"WhatsApp mesajı başarıyla gönderildi: {normalizedPhone}, MessageId: {messageId}");
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = true,
                        Message = "WhatsApp mesajı başarıyla gönderildi.",
                        Data = dto
                    };
                }
                else
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = errorMessage ?? "WhatsApp mesajı gönderilemedi.",
                        Data = dto
                    };
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"WhatsApp gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetWhatsAppHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"WhatsApp gönderilirken hata: {ex.Message}"
                };
            }
        }

        // Helper methods
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // WhatsApp format: 905551234567 (ülke kodu + telefon numarası, başında + olmadan)
            var pattern = @"^[1-9]\d{10,14}$";
            return Regex.IsMatch(phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", ""), pattern);
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // +, boşluk ve tire karakterlerini kaldır
            var normalized = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");

            // Türkiye için özel kontrol: 0 ile başlıyorsa 90 ile değiştir
            if (normalized.StartsWith("0") && normalized.Length == 11)
            {
                normalized = "90" + normalized.Substring(1);
            }
            // Türkiye için: +90 veya 90 ile başlamıyorsa 90 ekle
            else if (!normalized.StartsWith("90") && normalized.Length == 10)
            {
                normalized = "90" + normalized;
            }

            return normalized;
        }

        private async Task<(bool Success, string? MessageId, string? GatewayResponse, string? ErrorMessage)> SendWhatsAppToGatewayAsync(
            SendWhatsAppDto whatsAppDto, string normalizedPhone)
        {
            try
            {
                // Meta WhatsApp Business API entegrasyonu
                // Bu kısım gerçek API key ile çalışacak şekilde implement edilecek
                
                if (_whatsAppProvider == "Meta" && !string.IsNullOrEmpty(_whatsAppAccessToken))
                {
                    // TODO: Gerçek Meta WhatsApp Business API entegrasyonu
                    // Şimdilik mock implementasyon
                    _logger.LogInformation($"WhatsApp mesajı gönderiliyor (Mock): {normalizedPhone}");
                    
                    // Simüle edilmiş başarılı gönderim
                    await Task.Delay(100); // API çağrısını simüle et
                    
                    var mockMessageId = $"wamid.{Guid.NewGuid():N}";
                    var mockResponse = JsonSerializer.Serialize(new
                    {
                        messaging_product = "whatsapp",
                        contacts = new[] { new { input = normalizedPhone, wa_id = normalizedPhone } },
                        messages = new[] { new { id = mockMessageId } }
                    });

                    return (true, mockMessageId, mockResponse, null);
                }
                else
                {
                    _logger.LogWarning("WhatsApp API yapılandırması eksik veya devre dışı");
                    return (false, null, null, "WhatsApp API yapılandırması eksik");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp gateway'e mesaj gönderilirken hata");
                return (false, null, null, ex.Message);
            }
        }

        public async Task<ServiceMessage<GetWhatsAppHistoryDto>> SendTransferReminderAsync(int transferId, int hoursBefore = 24)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .FirstOrDefaultAsync(t => t.Id == transferId && !t.IsDeleted);

                if (transfer == null)
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Transfer bulunamadı."
                    };
                }

                if (transfer.Guest == null || string.IsNullOrEmpty(transfer.Guest.PhoneNumber))
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Misafir telefon numarası bulunamadı."
                    };
                }

                var transferDate = transfer.TransferDate;
                var reminderTime = transferDate.AddHours(-hoursBefore);

                if (reminderTime < DateTime.UtcNow)
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Hatırlatma zamanı geçmiş."
                    };
                }

                var message = $"Sayın {transfer.Guest.FullName}, {transferDate:dd.MM.yyyy HH:mm} tarihinde transferiniz var. " +
                             $"Kalkış: {transfer.PickupAddress}, Varış: {transfer.DropoffAddress}. " +
                             $"GuestFlow";

                var whatsAppDto = new SendWhatsAppDto
                {
                    PhoneNumber = transfer.Guest.PhoneNumber,
                    Message = message,
                    GuestId = transfer.GuestId,
                    RelatedEntityType = "Transfer",
                    RelatedEntityId = transferId,
                    TemplateName = "TransferReminder",
                    MessageType = WhatsAppMessageType.Text
                };

                return await SendWhatsAppAsync(whatsAppDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer hatırlatma WhatsApp mesajı gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetWhatsAppHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"Transfer hatırlatma WhatsApp mesajı gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GetWhatsAppHistoryDto>> SendTourReminderAsync(string tourType, int tourId, int hoursBefore = 24)
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
                        return new ServiceMessage<GetWhatsAppHistoryDto>
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
                        return new ServiceMessage<GetWhatsAppHistoryDto>
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
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Geçersiz tur tipi."
                    };
                }

                if (guest == null || string.IsNullOrEmpty(guest.PhoneNumber))
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Misafir telefon numarası bulunamadı."
                    };
                }

                var reminderTime = tourDate.AddHours(-hoursBefore);
                if (reminderTime < DateTime.UtcNow)
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Hatırlatma zamanı geçmiş."
                    };
                }

                var message = $"Sayın {guest.FullName}, {tourDate:dd.MM.yyyy HH:mm} tarihinde {tourName} rezervasyonunuz var. " +
                             $"Lokasyon: {location}. GuestFlow";

                var whatsAppDto = new SendWhatsAppDto
                {
                    PhoneNumber = guest.PhoneNumber,
                    Message = message,
                    GuestId = guest.Id,
                    RelatedEntityType = tourType,
                    RelatedEntityId = tourId,
                    TemplateName = "TourReminder",
                    MessageType = WhatsAppMessageType.Text
                };

                return await SendWhatsAppAsync(whatsAppDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur hatırlatma WhatsApp mesajı gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetWhatsAppHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"Tur hatırlatma WhatsApp mesajı gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GetWhatsAppHistoryDto>> SendReservationConfirmationAsync(int reservationId)
        {
            try
            {
                var reservation = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Rezervasyon bulunamadı."
                    };
                }

                if (reservation.Guest == null || string.IsNullOrEmpty(reservation.Guest.PhoneNumber))
                {
                    return new ServiceMessage<GetWhatsAppHistoryDto>
                    {
                        IsSuccess = false,
                        Message = "Misafir telefon numarası bulunamadı."
                    };
                }

                var message = $"Sayın {reservation.Guest.FullName}, rezervasyonunuz onaylandı. " +
                             $"Rezervasyon No: {reservation.ReservationNumber}, " +
                             $"Tarih: {reservation.ReservationDate:dd.MM.yyyy HH:mm}, " +
                             $"Tutar: {reservation.TotalAmount} {reservation.Currency}. GuestFlow";

                var whatsAppDto = new SendWhatsAppDto
                {
                    PhoneNumber = reservation.Guest.PhoneNumber,
                    Message = message,
                    GuestId = reservation.GuestId,
                    RelatedEntityType = "Reservation",
                    RelatedEntityId = reservationId,
                    TemplateName = "ReservationConfirmation",
                    MessageType = WhatsAppMessageType.Text
                };

                return await SendWhatsAppAsync(whatsAppDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon onay WhatsApp mesajı gönderilirken hata: {ex.Message}");
                return new ServiceMessage<GetWhatsAppHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"Rezervasyon onay WhatsApp mesajı gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<GetWhatsAppHistoryDto?> GetWhatsAppHistoryByIdAsync(int id)
        {
            try
            {
                var whatsApp = await _whatsAppHistoryRepository.GetAll()
                    .Include(w => w.Guest)
                    .Include(w => w.Personnel)
                    .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

                if (whatsApp == null)
                    return null;

                var dto = _mapper.Map<GetWhatsAppHistoryDto>(whatsApp);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WhatsApp geçmişi getirilirken hata: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedResult<GetWhatsAppHistoryDto>> GetWhatsAppHistoryPagedAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _whatsAppHistoryRepository.GetAll(x => !x.IsDeleted)
                    .Include(w => w.Guest)
                    .Include(w => w.Personnel)
                    .AsQueryable();

                // Filtreleme
                if (filters != null)
                {
                    if (filters.ContainsKey("GuestId") && filters["GuestId"] is int guestId)
                        query = query.Where(w => w.GuestId == guestId);

                    if (filters.ContainsKey("Status") && filters["Status"] is string status)
                        query = query.Where(w => w.Status == status);

                    if (filters.ContainsKey("StartDate") && filters["StartDate"] is DateTime startDate)
                        query = query.Where(w => w.SentDate >= startDate);

                    if (filters.ContainsKey("EndDate") && filters["EndDate"] is DateTime endDate)
                        query = query.Where(w => w.SentDate <= endDate);
                }

                // Sıralama
                if (sorting != null && !string.IsNullOrEmpty(sorting.SortBy))
                {
                    var sortOrder = sorting.SortOrder ?? "asc";
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(w => EF.Property<object>(w, sorting.SortBy))
                        : query.OrderBy(w => EF.Property<object>(w, sorting.SortBy));
                }
                else
                {
                    query = query.OrderByDescending(w => w.SentDate);
                }

                // Toplam kayıt sayısı
                var totalCount = await query.CountAsync();

                // Sayfalama
                var whatsAppList = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetWhatsAppHistoryDto>>(whatsAppList);

                return new PagedResult<GetWhatsAppHistoryDto>
                {
                    Data = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WhatsApp geçmişi getirilirken hata: {ex.Message}");
                return new PagedResult<GetWhatsAppHistoryDto>
                {
                    Data = new List<GetWhatsAppHistoryDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public async Task<List<GetWhatsAppHistoryDto>> GetWhatsAppHistoryByGuestIdAsync(int guestId)
        {
            try
            {
                var whatsAppList = await _whatsAppHistoryRepository.GetAll(x => x.GuestId == guestId && !x.IsDeleted)
                    .Include(w => w.Guest)
                    .Include(w => w.Personnel)
                    .OrderByDescending(w => w.SentDate)
                    .ToListAsync();

                return _mapper.Map<List<GetWhatsAppHistoryDto>>(whatsAppList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir WhatsApp geçmişi getirilirken hata: {ex.Message}");
                return new List<GetWhatsAppHistoryDto>();
            }
        }

        public async Task<List<GetWhatsAppHistoryDto>> GetWhatsAppHistoryByStatusAsync(string status)
        {
            try
            {
                var validStatuses = new[] { "Pending", "Sent", "Delivered", "Read", "Failed" };
                if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                    return new List<GetWhatsAppHistoryDto>();

                var whatsAppList = await _whatsAppHistoryRepository.GetAll(x => x.Status == status && !x.IsDeleted)
                    .Include(w => w.Guest)
                    .Include(w => w.Personnel)
                    .OrderByDescending(w => w.SentDate)
                    .ToListAsync();

                return _mapper.Map<List<GetWhatsAppHistoryDto>>(whatsAppList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Durum bazlı WhatsApp geçmişi getirilirken hata: {ex.Message}");
                return new List<GetWhatsAppHistoryDto>();
            }
        }

        public async Task<ServiceMessage> UpdateWhatsAppStatusAsync(int whatsAppId, string status, string? messageId = null, string? gatewayResponse = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var whatsApp = await _whatsAppHistoryRepository.GetAsync(x => x.Id == whatsAppId && !x.IsDeleted);
                if (whatsApp == null)
                    return new ServiceMessage { IsSuccess = false, Message = "WhatsApp kaydı bulunamadı." };

                var validStatuses = new[] { "Pending", "Sent", "Delivered", "Read", "Failed" };
                if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                    return new ServiceMessage { IsSuccess = false, Message = "Geçersiz WhatsApp durumu." };

                whatsApp.Status = status;
                if (!string.IsNullOrEmpty(messageId))
                    whatsApp.MessageId = messageId;
                if (!string.IsNullOrEmpty(gatewayResponse))
                    whatsApp.GatewayResponse = gatewayResponse;

                if (status == "Delivered" && whatsApp.DeliveredDate == null)
                    whatsApp.DeliveredDate = DateTime.UtcNow;
                else if (status == "Read" && whatsApp.ReadDate == null)
                    whatsApp.ReadDate = DateTime.UtcNow;

                await _whatsAppHistoryRepository.UpdateAsync(whatsApp);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"WhatsApp durumu güncellendi: {whatsAppId}, Status: {status}");
                return new ServiceMessage { IsSuccess = true, Message = "WhatsApp durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"WhatsApp durumu güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"WhatsApp durumu güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<WhatsAppStatisticsDto> GetWhatsAppStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _whatsAppHistoryRepository.GetAll(x => !x.IsDeleted).AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(w => w.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(w => w.SentDate <= endDate.Value);

                var whatsAppList = await query.ToListAsync();

                var statistics = new WhatsAppStatisticsDto
                {
                    TotalSent = whatsAppList.Count,
                    TotalDelivered = whatsAppList.Count(w => w.Status == "Delivered"),
                    TotalRead = whatsAppList.Count(w => w.Status == "Read"),
                    TotalFailed = whatsAppList.Count(w => w.Status == "Failed"),
                    TotalPending = whatsAppList.Count(w => w.Status == "Pending")
                };

                if (statistics.TotalSent > 0)
                {
                    statistics.SuccessRate = (decimal)(statistics.TotalDelivered + statistics.TotalRead) / statistics.TotalSent * 100;
                    statistics.DeliveryRate = (decimal)statistics.TotalDelivered / statistics.TotalSent * 100;
                    statistics.ReadRate = (decimal)statistics.TotalRead / statistics.TotalSent * 100;
                }

                // Mesaj tipine göre gruplama
                statistics.MessagesByType = whatsAppList
                    .Where(w => !string.IsNullOrEmpty(w.MessageType))
                    .GroupBy(w => w.MessageType)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Duruma göre gruplama
                statistics.MessagesByStatus = whatsAppList
                    .GroupBy(w => w.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WhatsApp istatistikleri getirilirken hata: {ex.Message}");
                return new WhatsAppStatisticsDto();
            }
        }
    }
}
