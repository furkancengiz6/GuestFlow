// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.OTA.BookingDotCom;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// Booking.com adapter - Booking.com API entegrasyonu
    /// </summary>
    public class BookingComAdapter : BaseOTAAdapter
    {
        private readonly IBookingDotComService _bookingService;

        public BookingComAdapter(
            OTAIntegration integration,
            IHttpClientFactory httpClientFactory,
            ILogger<BookingComAdapter> logger,
            IBookingDotComService bookingService)
            : base(integration, httpClientFactory, logger)
        {
            _bookingService = bookingService;
        }

        protected override void AddAuthenticationHeaders(HttpClient client)
        {
            // Booking.com API authentication
            // Booking.com genellikle API key veya OAuth 2.0 kullanır
            if (!string.IsNullOrEmpty(_integration.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _integration.AccessToken);
            }
            else if (!string.IsNullOrEmpty(_integration.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", _integration.ApiKey);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "GuestFlow/1.0");
        }

        public override async Task<bool> RefreshAccessTokenAsync()
        {
            // Booking.com integration typically uses static API keys or Basic Auth.
            // If OAuth is used in the future, implement the token exchange here.
            // For now, we assume the API Key configured is sufficient and valid.
            return await Task.FromResult(true);
        }

        public override async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Booking.com API health check endpoint
                var response = await CallApiAsync<object>("/api/v2/health", HttpMethod.Get);
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        public override async Task<List<OTAReservationDto>> GetReservationsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Booking.com API: GET /api/v2/reservations?startDate=...&endDate=...
                var endpoint = $"/api/v2/reservations?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var bookingReservations = await CallApiAsync<List<BookingReservationResponse>>(endpoint, HttpMethod.Get);
                
                if (bookingReservations == null) return new List<OTAReservationDto>();

                return bookingReservations.Select(MapBookingReservationToOTAReservation).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservations from Booking.com");
                throw;
            }
        }

        public override async Task<OTAReservationDto?> GetReservationAsync(string otaReservationId)
        {
            try
            {
                // Booking.com API: GET /api/v2/reservations/{reservationId}
                var endpoint = $"/api/v2/reservations/{otaReservationId}";
                var bookingReservation = await CallApiAsync<BookingReservationResponse>(endpoint, HttpMethod.Get);
                
                if (bookingReservation == null) return null;

                return MapBookingReservationToOTAReservation(bookingReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservation from Booking.com: {ReservationId}", otaReservationId);
                throw;
            }
        }

        public override async Task<bool> UpdateAvailabilityAsync(string roomTypeId, DateTime date, bool isAvailable)
        {
            try
            {
                // Booking.com API: PUT /api/v2/availability
                var endpoint = "/api/v2/availability";
                var request = new
                {
                    roomTypeId = roomTypeId,
                    date = date.ToString("yyyy-MM-dd"),
                    available = isAvailable
                };

                await CallApiAsync<object>(endpoint, HttpMethod.Put, request);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update availability on Booking.com: RoomTypeId={RoomTypeId}, Date={Date}", roomTypeId, date);
                return false;
            }
        }

        public override async Task<bool> UpdateRatesAsync(string roomTypeId, DateTime date, decimal price, string currency)
        {
            try
            {
                // Booking.com API: PUT /api/v2/rates
                var endpoint = "/api/v2/rates";
                var request = new
                {
                    roomTypeId = roomTypeId,
                    date = date.ToString("yyyy-MM-dd"),
                    price = price,
                    currency = currency
                };

                await CallApiAsync<object>(endpoint, HttpMethod.Put, request);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update rates on Booking.com: RoomTypeId={RoomTypeId}, Date={Date}", roomTypeId, date);
                return false;
            }
        }

        public override async Task<bool> ProcessWebhookAsync(string payload, string? signature = null)
        {
            try
            {
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Booking.com webhook missing signature");
                    return false;
                }

                // Verify HMAC signature using the service
                if (!string.IsNullOrEmpty(_integration.ApiSecret))
                {
                    if (!_bookingService.ValidateSignature(payload, signature, _integration.ApiSecret))
                    {
                         _logger.LogWarning("Invalid Booking.com webhook signature");
                         return false;
                    }
                }

                // Parse webhook payload using the service
                // Note: We might want to use the parsed object for something, but for now we just parse to validate JSON structure
                var webhookData = _bookingService.ParsePayload(payload);
                
                _logger.LogInformation("Successfully verified and parsed Booking.com webhook");

                // Here we would typically queue the event for processing
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Booking.com webhook");
                return false;
            }
        }

        private OTAReservationDto MapBookingReservationToOTAReservation(BookingReservationResponse bookingReservation)
        {
            return new OTAReservationDto
            {
                OTAReservationId = bookingReservation.ReservationId ?? string.Empty,
                OTAHotelId = bookingReservation.HotelId ?? string.Empty,
                OTARoomTypeId = bookingReservation.RoomTypeId ?? string.Empty,
                CheckInDate = bookingReservation.CheckInDate,
                CheckOutDate = bookingReservation.CheckOutDate,
                GuestCount = bookingReservation.GuestCount ?? 1,
                TotalPrice = bookingReservation.TotalPrice ?? 0,
                Currency = bookingReservation.Currency ?? "TRY",
                GuestName = bookingReservation.GuestName ?? string.Empty,
                GuestEmail = bookingReservation.GuestEmail,
                GuestPhone = bookingReservation.GuestPhone,
                Status = bookingReservation.Status ?? "Confirmed",
                OTACreatedDate = bookingReservation.CreatedDate ?? DateTime.UtcNow,
                OTALastModifiedDate = bookingReservation.LastModifiedDate
            };
        }

        // Booking.com API response models (temporary - gerçek API response'larına göre güncellenecek)
        private class BookingReservationResponse
        {
            public string? ReservationId { get; set; }
            public string? HotelId { get; set; }
            public string? RoomTypeId { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public int? GuestCount { get; set; }
            public decimal? TotalPrice { get; set; }
            public string? Currency { get; set; }
            public string? GuestName { get; set; }
            public string? GuestEmail { get; set; }
            public string? GuestPhone { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedDate { get; set; }
            public DateTime? LastModifiedDate { get; set; }
        }
    }
}
