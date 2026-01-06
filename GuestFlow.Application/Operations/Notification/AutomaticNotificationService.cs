using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Notification.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Notification
{
    /// <summary>
    /// Otomatik bildirim servisi - Transfer, Tur, Rezervasyon gibi olaylar için otomatik bildirimler gönderir
    /// </summary>
    public class AutomaticNotificationService : IAutomaticNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservationRepository;
        private readonly IRepository<ItineraryEntity> _itineraryRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AutomaticNotificationService> _logger;

        public AutomaticNotificationService(
            INotificationService notificationService,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<RestaurantReservationEntity> restaurantReservationRepository,
            IRepository<ItineraryEntity> itineraryRepository,
            IRepository<GuestEntity> guestRepository,
            IEmailService emailService,
            ILogger<AutomaticNotificationService> logger)
        {
            _notificationService = notificationService;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _restaurantReservationRepository = restaurantReservationRepository;
            _itineraryRepository = itineraryRepository;
            _guestRepository = guestRepository;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Transfer oluşturulduğunda otomatik bildirim gönderir
        /// </summary>
        public async Task<ServiceMessage> NotifyTransferCreatedAsync(int transferId)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer?.Guest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer veya misafir bulunamadı." };

                var title = "Transfer Rezervasyonunuz Oluşturuldu";
                var content = $@"
Merhaba {transfer.Guest.FullName},

Transfer rezervasyonunuz başarıyla oluşturuldu.

Detaylar:
- Tarih: {transfer.TransferDate:dd.MM.yyyy HH:mm}
- Kalkış: {transfer.PickupAddress}
- Varış: {transfer.DropoffAddress}
- Fiyat: {transfer.FinalPrice} {transfer.Currency}

Rezervasyon ID: {transfer.Id}

İyi yolculuklar dileriz.
";

                var notification = new CreateNotificationDto
                {
                    Title = title,
                    Content = content,
                    NotificationType = "Email",
                    RecipientEmail = transfer.Guest.Email,
                    RecipientGuestId = transfer.GuestId,
                    RelatedEntityType = "Transfer",
                    RelatedEntityId = transferId,
                    TemplateName = "TransferCreated"
                };

                var result = await _notificationService.CreateAndSendNotificationAsync(notification);
                return new ServiceMessage 
                { 
                    IsSuccess = result.IsSuccess, 
                    Message = result.Message 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer bildirimi gönderilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Bildirim gönderilirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Transfer durumu değiştiğinde otomatik bildirim gönderir
        /// </summary>
        public async Task<ServiceMessage> NotifyTransferStatusChangedAsync(int transferId, string oldStatus, string newStatus)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer?.Guest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer veya misafir bulunamadı." };

                var statusText = newStatus switch
                {
                    "Pending" => "Beklemede",
                    "InProgress" => "Devam Ediyor",
                    "Completed" => "Tamamlandı",
                    "Cancelled" => "İptal Edildi",
                    _ => newStatus
                };

                var title = $"Transfer Durumu Güncellendi: {statusText}";
                var content = $@"
Merhaba {transfer.Guest.FullName},

Transfer rezervasyonunuzun durumu güncellendi.

Eski Durum: {oldStatus}
Yeni Durum: {statusText}

Transfer Detayları:
- Tarih: {transfer.TransferDate:dd.MM.yyyy HH:mm}
- Kalkış: {transfer.PickupAddress}
- Varış: {transfer.DropoffAddress}

Sorularınız için bizimle iletişime geçebilirsiniz.
";

                var notification = new CreateNotificationDto
                {
                    Title = title,
                    Content = content,
                    NotificationType = "Email",
                    RecipientEmail = transfer.Guest.Email,
                    RecipientGuestId = transfer.GuestId,
                    RelatedEntityType = "Transfer",
                    RelatedEntityId = transferId,
                    TemplateName = "TransferStatusChanged"
                };

                var result = await _notificationService.CreateAndSendNotificationAsync(notification);
                return new ServiceMessage 
                { 
                    IsSuccess = result.IsSuccess, 
                    Message = result.Message 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer durum bildirimi gönderilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Bildirim gönderilirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// İtinerary oluşturulduğunda otomatik bildirim gönderir
        /// </summary>
        public async Task<ServiceMessage> NotifyItineraryCreatedAsync(int itineraryId)
        {
            try
            {
                var itinerary = await _itineraryRepository.GetAll()
                    .Include(i => i.Guest)
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Id == itineraryId);

                if (itinerary?.Guest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "İtinerary veya misafir bulunamadı." };

                var title = "Seyahat Planınız Hazırlandı";
                var content = $@"
Merhaba {itinerary.Guest.FullName},

Seyahat planınız (İtinerary) başarıyla oluşturuldu.

Plan Detayları:
- Plan Numarası: {itinerary.ItineraryNumber}
- Başlangıç: {itinerary.StartDate:dd.MM.yyyy}
- Bitiş: {itinerary.EndDate:dd.MM.yyyy}
- Toplam Maliyet: {itinerary.TotalCost} {itinerary.Currency}
- Durum: {itinerary.Status}

Planınızda {itinerary.Items.Count} adet aktivite bulunmaktadır.

Detaylı bilgi için lütfen sistemimize giriş yapın veya bizimle iletişime geçin.

İyi tatiller dileriz!
";

                var notification = new CreateNotificationDto
                {
                    Title = title,
                    Content = content,
                    NotificationType = "Email",
                    RecipientEmail = itinerary.Guest.Email,
                    RecipientGuestId = itinerary.GuestId,
                    RelatedEntityType = "Itinerary",
                    RelatedEntityId = itineraryId,
                    TemplateName = "ItineraryCreated"
                };

                var result = await _notificationService.CreateAndSendNotificationAsync(notification);
                return new ServiceMessage 
                { 
                    IsSuccess = result.IsSuccess, 
                    Message = result.Message 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"İtinerary bildirimi gönderilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Bildirim gönderilirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Restoran rezervasyonu oluşturulduğunda otomatik bildirim gönderir
        /// </summary>
        public async Task<ServiceMessage> NotifyRestaurantReservationCreatedAsync(int reservationId)
        {
            try
            {
                var reservation = await _restaurantReservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Restaurant)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                if (reservation?.Guest == null || reservation.Restaurant == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon, misafir veya restoran bulunamadı." };

                var title = "Restoran Rezervasyonunuz Onaylandı";
                var content = $@"
Merhaba {reservation.Guest.FullName},

Restoran rezervasyonunuz başarıyla oluşturuldu.

Rezervasyon Detayları:
- Restoran: {reservation.Restaurant.RestaurantName}
- Adres: {reservation.Restaurant.Address}
- Tarih: {reservation.ReservationDate:dd.MM.yyyy}
- Saat: {reservation.ReservationTime:hh\:mm}
- Misafir Sayısı: {reservation.NumberOfGuests}
- Onay Numarası: {reservation.ConfirmationNumber ?? "N/A"}

{(reservation.SpecialRequests != null ? $"Özel İstekleriniz: {reservation.SpecialRequests}\n" : "")}

Restorana zamanında gelmenizi rica ederiz.

Afiyet olsun!
";

                var notification = new CreateNotificationDto
                {
                    Title = title,
                    Content = content,
                    NotificationType = "Email",
                    RecipientEmail = reservation.Guest.Email,
                    RecipientGuestId = reservation.GuestId,
                    RelatedEntityType = "RestaurantReservation",
                    RelatedEntityId = reservationId,
                    TemplateName = "RestaurantReservationCreated"
                };

                var result = await _notificationService.CreateAndSendNotificationAsync(notification);
                return new ServiceMessage 
                { 
                    IsSuccess = result.IsSuccess, 
                    Message = result.Message 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Restoran rezervasyon bildirimi gönderilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Bildirim gönderilirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Yaklaşan aktiviteler için hatırlatma bildirimi gönderir
        /// </summary>
        public async Task<ServiceMessage> SendUpcomingActivityRemindersAsync()
        {
            try
            {
                var tomorrow = DateTime.UtcNow.AddDays(1).Date;
                var dayAfterTomorrow = DateTime.UtcNow.AddDays(2).Date;

                var remindersSent = 0;

                // Yarın yapılacak transferler
                var tomorrowTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Where(t => t.TransferDate.Date == tomorrow && !t.IsDeleted)
                    .ToListAsync();

                foreach (var transfer in tomorrowTransfers)
                {
                    if (transfer.Guest?.Email != null)
                    {
                        var title = "Yarın Transferiniz Var - Hatırlatma";
                        var content = $@"
Merhaba {transfer.Guest.FullName},

Yarın ({transfer.TransferDate:dd.MM.yyyy HH:mm}) transferiniz bulunmaktadır.

Transfer Detayları:
- Kalkış: {transfer.PickupAddress}
- Varış: {transfer.DropoffAddress}

Lütfen zamanında hazır olun.

İyi yolculuklar!
";

                        var notification = new CreateNotificationDto
                        {
                            Title = title,
                            Content = content,
                            NotificationType = "Email",
                            RecipientEmail = transfer.Guest.Email,
                            RecipientGuestId = transfer.GuestId,
                            RelatedEntityType = "Transfer",
                            RelatedEntityId = transfer.Id,
                            TemplateName = "TransferReminder"
                        };

                        await _notificationService.CreateAndSendNotificationAsync(notification);
                        remindersSent++;
                    }
                }

                _logger.LogInformation($"Yaklaşan aktivite hatırlatmaları gönderildi: {remindersSent} bildirim");
                return new ServiceMessage 
                { 
                    IsSuccess = true, 
                    Message = $"{remindersSent} adet hatırlatma bildirimi gönderildi." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hatırlatma bildirimleri gönderilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Bildirim gönderilirken hata: {ex.Message}" };
            }
        }
    }
}

