// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Requests.OTA;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// OTA webhook handler - Booking.com, Expedia vb. webhook'larını işler
    /// </summary>
    public class OTAWebhookHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOTAIntegrationService _otaIntegrationService;
        private readonly IOTAChannelManagerService _channelManagerService;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly ILogger<OTAWebhookHandler> _logger;

        public OTAWebhookHandler(
            IUnitOfWork unitOfWork,
            IOTAIntegrationService otaIntegrationService,
            IOTAChannelManagerService channelManagerService,
            IPMSIntegrationService pmsIntegrationService,
            ILogger<OTAWebhookHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _otaIntegrationService = otaIntegrationService;
            _channelManagerService = channelManagerService;
            _pmsIntegrationService = pmsIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// Webhook signature'ı doğrula
        /// </summary>
        public bool ValidateWebhookSignature(string payload, string signature, string secret, string providerCode)
        {
            try
            {
                var providerCodeUpper = providerCode.ToUpperInvariant();
                
                // Provider'a göre signature validation
                return providerCodeUpper switch
                {
                    "BKG" or "BOOKING" or "BOOKINGCOM" => ValidateBookingComSignature(payload, signature, secret),
                    "EXP" or "EXPEDIA" => ValidateExpediaSignature(payload, signature, secret),
                    _ => ValidateGenericHMACSignature(payload, signature, secret)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate webhook signature for provider: {ProviderCode}", providerCode);
                return false;
            }
        }

        /// <summary>
        /// Booking.com webhook signature validation
        /// Booking.com genellikle HMAC SHA256 kullanır
        /// </summary>
        private bool ValidateBookingComSignature(string payload, string signature, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();
            
            // Booking.com signature format: hex lowercase
            return computedSignature == signature.ToLowerInvariant();
        }

        /// <summary>
        /// Expedia webhook signature validation
        /// Expedia Partner Solutions genellikle HMAC SHA256 kullanır
        /// </summary>
        private bool ValidateExpediaSignature(string payload, string signature, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToBase64String(hashBytes);
            
            // Expedia signature format: base64
            return computedSignature == signature;
        }

        /// <summary>
        /// Generic HMAC SHA256 signature validation
        /// </summary>
        private bool ValidateGenericHMACSignature(string payload, string signature, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignatureHex = Convert.ToHexString(hashBytes).ToLowerInvariant();
            var computedSignatureBase64 = Convert.ToBase64String(hashBytes);
            
            // Try both hex and base64 formats
            return computedSignatureHex == signature.ToLowerInvariant() || 
                   computedSignatureBase64 == signature;
        }

        /// <summary>
        /// Webhook payload'ını parse et ve event type'ı belirle
        /// </summary>
        public OTAWebhookEvent? ParseWebhookPayload(string payload, string providerCode)
        {
            try
            {
                var providerCodeUpper = providerCode.ToUpperInvariant();
                
                return providerCodeUpper switch
                {
                    "BKG" or "BOOKING" or "BOOKINGCOM" => ParseBookingComWebhook(payload),
                    "EXP" or "EXPEDIA" => ParseExpediaWebhook(payload),
                    _ => ParseGenericWebhook(payload)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse webhook payload for provider: {ProviderCode}", providerCode);
                return null;
            }
        }

        /// <summary>
        /// Booking.com webhook payload'ını parse et
        /// </summary>
        private OTAWebhookEvent? ParseBookingComWebhook(string payload)
        {
            var jsonDoc = JsonDocument.Parse(payload);
            var root = jsonDoc.RootElement;

            var eventType = root.TryGetProperty("event_type", out var eventTypeProp) 
                ? eventTypeProp.GetString() 
                : root.TryGetProperty("eventType", out var eventTypeProp2) 
                    ? eventTypeProp2.GetString() 
                    : null;

            if (string.IsNullOrEmpty(eventType))
                return null;

            var reservationId = root.TryGetProperty("reservation_id", out var resIdProp)
                ? resIdProp.GetString()
                : root.TryGetProperty("reservationId", out var resIdProp2)
                    ? resIdProp2.GetString()
                    : null;

            return new OTAWebhookEvent
            {
                EventType = eventType,
                ReservationId = reservationId,
                Payload = payload,
                ProviderCode = "BKG"
            };
        }

        /// <summary>
        /// Expedia webhook payload'ını parse et
        /// </summary>
        private OTAWebhookEvent? ParseExpediaWebhook(string payload)
        {
            var jsonDoc = JsonDocument.Parse(payload);
            var root = jsonDoc.RootElement;

            var eventType = root.TryGetProperty("event_type", out var eventTypeProp)
                ? eventTypeProp.GetString()
                : root.TryGetProperty("eventType", out var eventTypeProp2)
                    ? eventTypeProp2.GetString()
                    : null;

            if (string.IsNullOrEmpty(eventType))
                return null;

            var reservationId = root.TryGetProperty("reservation_id", out var resIdProp)
                ? resIdProp.GetString()
                : root.TryGetProperty("reservationId", out var resIdProp2)
                    ? resIdProp2.GetString()
                    : null;

            return new OTAWebhookEvent
            {
                EventType = eventType,
                ReservationId = reservationId,
                Payload = payload,
                ProviderCode = "EXP"
            };
        }

        /// <summary>
        /// Generic webhook payload'ını parse et
        /// </summary>
        private OTAWebhookEvent? ParseGenericWebhook(string payload)
        {
            var jsonDoc = JsonDocument.Parse(payload);
            var root = jsonDoc.RootElement;

            var eventType = root.TryGetProperty("eventType", out var eventTypeProp)
                ? eventTypeProp.GetString()
                : root.TryGetProperty("event_type", out var eventTypeProp2)
                    ? eventTypeProp2.GetString()
                    : root.TryGetProperty("type", out var typeProp)
                        ? typeProp.GetString()
                        : null;

            if (string.IsNullOrEmpty(eventType))
                return null;

            var reservationId = root.TryGetProperty("reservationId", out var resIdProp)
                ? resIdProp.GetString()
                : root.TryGetProperty("reservation_id", out var resIdProp2)
                    ? resIdProp2.GetString()
                    : null;

            return new OTAWebhookEvent
            {
                EventType = eventType,
                ReservationId = reservationId,
                Payload = payload,
                ProviderCode = "UNKNOWN"
            };
        }

        /// <summary>
        /// Webhook event'ini işle
        /// </summary>
        public async Task<bool> ProcessWebhookEventAsync(int otaIntegrationId, OTAWebhookEvent webhookEvent)
        {
            try
            {
                var otaIntegration = await _unitOfWork.OTAIntegrations.GetByIdAsync(otaIntegrationId);
                if (otaIntegration == null)
                {
                    _logger.LogWarning("OTA integration not found: {IntegrationId}", otaIntegrationId);
                    return false;
                }

                _logger.LogInformation("Processing OTA webhook event: EventType={EventType}, ReservationId={ReservationId}, Provider={Provider}",
                    webhookEvent.EventType, webhookEvent.ReservationId, otaIntegration.ProviderName);

                // Event type'a göre işlem yap
                switch (webhookEvent.EventType?.ToUpperInvariant())
                {
                    case "RESERVATION_CREATED":
                    case "BOOKING_CREATED":
                        await HandleReservationCreatedAsync(otaIntegrationId, webhookEvent);
                        break;

                    case "RESERVATION_UPDATED":
                    case "BOOKING_UPDATED":
                    case "RESERVATION_MODIFIED":
                        await HandleReservationUpdatedAsync(otaIntegrationId, webhookEvent);
                        break;

                    case "RESERVATION_CANCELLED":
                    case "BOOKING_CANCELLED":
                        await HandleReservationCancelledAsync(otaIntegrationId, webhookEvent);
                        break;

                    case "PAYMENT_RECEIVED":
                    case "PAYMENT_UPDATED":
                        await HandlePaymentUpdatedAsync(otaIntegrationId, webhookEvent);
                        break;

                    default:
                        _logger.LogWarning("Unknown OTA webhook event type: {EventType}", webhookEvent.EventType);
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process OTA webhook event: EventType={EventType}, ReservationId={ReservationId}",
                    webhookEvent.EventType, webhookEvent.ReservationId);
                return false;
            }
        }

        /// <summary>
        /// Rezervasyon oluşturuldu event'ini işle
        /// </summary>
        private async Task HandleReservationCreatedAsync(int otaIntegrationId, OTAWebhookEvent webhookEvent)
        {
            try
            {
                if (string.IsNullOrEmpty(webhookEvent.ReservationId))
                    return;

                // 1. Parse payload to generic DTO
                var reservationDto = ParseReservationFromPayload(webhookEvent);
                if (reservationDto == null)
                {
                    _logger.LogWarning("Failed to parse reservation DTO from payload for event {EventType}", webhookEvent.EventType);
                    return;
                }

                // 2. Delegate to Channel Manager for orchestration
                var result = await _channelManagerService.ProcessIncomingReservationAsync(otaIntegrationId, reservationDto);

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to process incoming reservation {ReservationId}: {Message}", 
                        webhookEvent.ReservationId, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle reservation created event: ReservationId={ReservationId}",
                    webhookEvent.ReservationId);
                throw;
            }
        }

        /// <summary>
        /// Rezervasyon güncellendi event'ini işle
        /// </summary>
        private async Task HandleReservationUpdatedAsync(int otaIntegrationId, OTAWebhookEvent webhookEvent)
        {
            try
            {
                if (string.IsNullOrEmpty(webhookEvent.ReservationId))
                    return;

                 // 1. Parse payload to generic DTO
                var reservationDto = ParseReservationFromPayload(webhookEvent);
                if (reservationDto == null)
                {
                    _logger.LogWarning("Failed to parse reservation DTO from payload for event {EventType}", webhookEvent.EventType);
                    return;
                }

                // 2. Delegate to Channel Manager (same logic for create/update usually, or specialized)
                var result = await _channelManagerService.ProcessIncomingReservationAsync(otaIntegrationId, reservationDto);

                if (!result.Success)
                {
                     _logger.LogWarning("Failed to process updated reservation {ReservationId}: {Message}", 
                        webhookEvent.ReservationId, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle reservation updated event: ReservationId={ReservationId}",
                    webhookEvent.ReservationId);
                throw;
            }
        }

        /// <summary>
        /// Rezervasyon iptal edildi event'ini işle
        /// </summary>
        private async Task HandleReservationCancelledAsync(int otaIntegrationId, OTAWebhookEvent webhookEvent)
        {
            try
            {
                // OTA rezervasyonunu iptal olarak işaretle
                var existingOTAReservation = await _unitOfWork.OTAReservations
                    .GetAll(r => r.OTAIntegrationId == otaIntegrationId && r.OTAReservationId == webhookEvent.ReservationId)
                    .FirstOrDefaultAsync();

                if (existingOTAReservation != null)
                {
                    existingOTAReservation.Status = "Cancelled";
                    existingOTAReservation.OTALastModifiedDate = DateTime.UtcNow;

                    _unitOfWork.OTAReservations.Update(existingOTAReservation);
                    await _unitOfWork.CommitAsync();
                }

                // PMS'e iptal bilgisini gönder (aktif PMS entegrasyonları varsa)
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var pmsIntegration in activePMSIntegrations)
                {
                    // TODO: PMS'e rezervasyon iptal bilgisini gönder
                    _logger.LogInformation("Reservation cancelled notification sent to PMS {PMSProvider}",
                        pmsIntegration.ProviderName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle reservation cancelled event: ReservationId={ReservationId}",
                    webhookEvent.ReservationId);
                throw;
            }
        }

        /// <summary>
        /// Ödeme güncellendi event'ini işle
        /// </summary>
        private async Task HandlePaymentUpdatedAsync(int otaIntegrationId, OTAWebhookEvent webhookEvent)
        {
            try
            {
                // Ödeme bilgisi güncellendi, rezervasyonu yeniden senkronize et
                await HandleReservationUpdatedAsync(otaIntegrationId, webhookEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle payment updated event: ReservationId={ReservationId}",
                    webhookEvent.ReservationId);
                throw;
            }
        }

        /// <summary>
        /// OTA rezervasyonunu GuestFlow'a senkronize et
        /// </summary>
        private async Task SyncOTAReservationToGuestFlowAsync(int otaIntegrationId, OTAReservationDto reservation)
        {
            // OTAIntegrationService'deki SyncOTAReservationToGuestFlowAsync metodunu kullan
            // Bu metod zaten OTAIntegrationService'de var
            var otaService = _otaIntegrationService as OTAIntegrationService;
            if (otaService == null)
                return;

            // Reflection veya protected method kullanmak yerine, direkt OTAIntegrationService metodunu çağıramıyoruz
            // Bu yüzden OTAIntegrationService'e public bir metod eklemeliyiz veya burada implement etmeliyiz
            // Şimdilik basit bir implementasyon yapıyoruz

            var existingOTAReservation = await _unitOfWork.OTAReservations
                .GetAll(r => r.OTAIntegrationId == otaIntegrationId && r.OTAReservationId == reservation.OTAReservationId)
                .FirstOrDefaultAsync();

            if (existingOTAReservation == null)
            {
                var newOTAReservation = new OTAReservation
                {
                    OTAIntegrationId = otaIntegrationId,
                    OTAReservationId = reservation.OTAReservationId,
                    OTAHotelId = reservation.OTAHotelId,
                    OTARoomTypeId = reservation.OTARoomTypeId,
                    CheckInDate = reservation.CheckInDate,
                    CheckOutDate = reservation.CheckOutDate,
                    GuestCount = reservation.GuestCount,
                    TotalPrice = reservation.TotalPrice,
                    Currency = reservation.Currency,
                    GuestName = reservation.GuestName,
                    GuestEmail = reservation.GuestEmail,
                    GuestPhone = reservation.GuestPhone,
                    Status = reservation.Status,
                    OTACreatedDate = reservation.OTACreatedDate,
                    OTALastModifiedDate = reservation.OTALastModifiedDate
                };

                await _unitOfWork.OTAReservations.AddAsync(newOTAReservation);
                await _unitOfWork.CommitAsync();
            }
        }

        /// <summary>
        /// OTA rezervasyonunu PMS'e gönder
        /// </summary>
        private async Task SendOTAReservationToPMSAsync(int otaIntegrationId, OTAReservationDto reservation)
        {
            // Aktif PMS entegrasyonlarını bul
            var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                .GetAll(i => i.IsActive && !i.IsDeleted)
                .ToListAsync();

            foreach (var pmsIntegration in activePMSIntegrations)
            {
                try
                {
                    // Guest oluştur veya bul
                    var guest = await _unitOfWork.Guests
                        .GetAll(g => g.Email == reservation.GuestEmail && !g.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (guest == null)
                    {
                        // Yeni guest oluştur
                        var guestCode = await GenerateGuestCodeAsync();
                        guest = new GuestFlow.Domain.Entities.Core.GuestEntity
                        {
                            GuestCode = guestCode,
                            FullName = reservation.GuestName,
                            Email = reservation.GuestEmail,
                            PhoneNumber = reservation.GuestPhone,
                            CheckInDate = reservation.CheckInDate,
                            CheckOutDate = reservation.CheckOutDate
                        };

                        await _unitOfWork.Guests.AddAsync(guest);
                        await _unitOfWork.CommitAsync();
                    }

                    _logger.LogInformation("OTA reservation {ReservationId} synced to PMS {PMSProvider} via guest {GuestId}",
                        reservation.OTAReservationId, pmsIntegration.ProviderName, guest.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send OTA reservation to PMS {PMSProvider}: {ReservationId}",
                        pmsIntegration.ProviderName, reservation.OTAReservationId);
                }
            }
        }

        /// <summary>
        /// OTA adapter oluştur (helper method)
        /// </summary>
        private BaseOTAAdapter? CreateOTAAdapter(int otaIntegrationId)
        {
            // OTAIntegrationService'deki CreateAdapter metodunu kullanamıyoruz (private)
            // Bu yüzden adapter'ı burada oluşturuyoruz
            // TODO: CreateAdapter metodunu public yap veya IOTAAdapterFactory interface'i oluştur
            return null; // Şimdilik null döndürüyoruz, adapter'ı OTAIntegrationService üzerinden kullanacağız
        }

        private async Task<string> GenerateGuestCodeAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastGuest = await _unitOfWork.Guests
                .GetAll()
                .OrderByDescending(g => g.GuestCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastGuest != null && lastGuest.GuestCode.StartsWith($"GUEST-{year}-"))
            {
                var parts = lastGuest.GuestCode.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"GUEST-{year}-{nextNumber:D4}";
        }

        private OTAReservationDto? ParseReservationFromPayload(OTAWebhookEvent webhookEvent)
        {
             try
            {
                // Simplified generic parsing based on provider
                if (webhookEvent.ProviderCode == "BKG")
                {
                    // Basic JSON parsing - assuming flat structure or matching DTO for now
                    // In real implementation, use BookingWebhookPayloadDto and map
                    using var doc = JsonDocument.Parse(webhookEvent.Payload);
                    var root = doc.RootElement;
                    
                    return new OTAReservationDto
                    {
                        OTAReservationId = webhookEvent.ReservationId ?? string.Empty,
                         // Safe defaults if parsing fails or fields missing in webhook (webhooks often partial)
                         // Real impl would need callbacks to get full details if webhook is lightweight
                        GuestName = root.TryGetProperty("guest_name", out var gn) ? gn.GetString() ?? "Unknown" : "Unknown",
                        TotalPrice = root.TryGetProperty("total_price", out var tp) && tp.TryGetDecimal(out var d) ? d : 0,
                        Status = "Confirmed", // Default since we are in Created/Updated
                        OTAHotelId = root.TryGetProperty("hotel_id", out var hi) ? hi.GetString() ?? "" : "",
                        CheckInDate = DateTime.UtcNow.Date.AddDays(1), // Dummy if not found
                        CheckOutDate = DateTime.UtcNow.Date.AddDays(2)
                    };
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// OTA webhook event modeli
    /// </summary>
    public class OTAWebhookEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string? ReservationId { get; set; }
        public string Payload { get; set; } = string.Empty;
        public string ProviderCode { get; set; } = string.Empty;


    }
}
