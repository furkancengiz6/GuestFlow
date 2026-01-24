// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// Expedia adapter - Expedia Partner Solutions (EPS) API entegrasyonu
    /// </summary>
    public class ExpediaAdapter : BaseOTAAdapter
    {
        public ExpediaAdapter(
            OTAIntegration integration,
            IHttpClientFactory httpClientFactory,
            ILogger<ExpediaAdapter> logger)
            : base(integration, httpClientFactory, logger)
        {
        }

        protected override void AddAuthenticationHeaders(HttpClient client)
        {
            // Expedia Partner Solutions (EPS) API authentication
            // EPS genellikle API key + secret veya OAuth 2.0 kullanır
            if (!string.IsNullOrEmpty(_integration.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _integration.AccessToken);
            }
            else if (!string.IsNullOrEmpty(_integration.ApiKey) && !string.IsNullOrEmpty(_integration.ApiSecret))
            {
                // Basic Auth veya custom header
                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{_integration.ApiKey}:{_integration.ApiSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "GuestFlow/1.0");
        }

        public override async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                // Expedia OAuth 2.0 token refresh
                _logger.LogInformation("Refreshing Expedia access token for integration {IntegrationId}", _integration.Id);
                
                // TODO: Expedia token refresh implementation
                // Expedia Partner Solutions API dokümantasyonuna göre implement edilecek
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Expedia access token");
                return false;
            }
        }

        public override async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Expedia Partner Solutions API health check endpoint
                var response = await CallApiAsync<object>("/api/v1/health", HttpMethod.Get);
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
                // Expedia Partner Solutions API: GET /api/v1/reservations?startDate=...&endDate=...
                var endpoint = $"/api/v1/reservations?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var expediaReservations = await CallApiAsync<List<ExpediaReservationResponse>>(endpoint, HttpMethod.Get);
                
                if (expediaReservations == null) return new List<OTAReservationDto>();

                return expediaReservations.Select(MapExpediaReservationToOTAReservation).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservations from Expedia");
                throw;
            }
        }

        public override async Task<OTAReservationDto?> GetReservationAsync(string otaReservationId)
        {
            try
            {
                // Expedia Partner Solutions API: GET /api/v1/reservations/{reservationId}
                var endpoint = $"/api/v1/reservations/{otaReservationId}";
                var expediaReservation = await CallApiAsync<ExpediaReservationResponse>(endpoint, HttpMethod.Get);
                
                if (expediaReservation == null) return null;

                return MapExpediaReservationToOTAReservation(expediaReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservation from Expedia: {ReservationId}", otaReservationId);
                throw;
            }
        }

        public override async Task<bool> UpdateAvailabilityAsync(string roomTypeId, DateTime date, bool isAvailable)
        {
            try
            {
                // Expedia Partner Solutions API: PUT /api/v1/inventory
                var endpoint = "/api/v1/inventory";
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
                _logger.LogError(ex, "Failed to update availability on Expedia: RoomTypeId={RoomTypeId}, Date={Date}", roomTypeId, date);
                return false;
            }
        }

        public override async Task<bool> UpdateRatesAsync(string roomTypeId, DateTime date, decimal price, string currency)
        {
            try
            {
                // Expedia Partner Solutions API: PUT /api/v1/rates
                var endpoint = "/api/v1/rates";
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
                _logger.LogError(ex, "Failed to update rates on Expedia: RoomTypeId={RoomTypeId}, Date={Date}", roomTypeId, date);
                return false;
            }
        }

        public override async Task<bool> ProcessWebhookAsync(string payload, string? signature = null)
        {
            try
            {
                // Expedia webhook signature validation
                if (!string.IsNullOrEmpty(signature))
                {
                    // TODO: Implement Expedia webhook signature validation
                }

                // Parse webhook payload
                var webhookData = JsonSerializer.Deserialize<Dictionary<string, object>>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null) return false;

                // Process webhook based on event type
                var eventType = webhookData.ContainsKey("eventType") ? webhookData["eventType"]?.ToString() : null;
                
                _logger.LogInformation("Processing Expedia webhook: EventType={EventType}", eventType);
                
                // TODO: Implement webhook processing logic
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Expedia webhook");
                return false;
            }
        }

        private OTAReservationDto MapExpediaReservationToOTAReservation(ExpediaReservationResponse expediaReservation)
        {
            return new OTAReservationDto
            {
                OTAReservationId = expediaReservation.ReservationId ?? string.Empty,
                OTAHotelId = expediaReservation.HotelId ?? string.Empty,
                OTARoomTypeId = expediaReservation.RoomTypeId ?? string.Empty,
                CheckInDate = expediaReservation.CheckInDate,
                CheckOutDate = expediaReservation.CheckOutDate,
                GuestCount = expediaReservation.GuestCount ?? 1,
                TotalPrice = expediaReservation.TotalPrice ?? 0,
                Currency = expediaReservation.Currency ?? "TRY",
                GuestName = expediaReservation.GuestName ?? string.Empty,
                GuestEmail = expediaReservation.GuestEmail,
                GuestPhone = expediaReservation.GuestPhone,
                Status = expediaReservation.Status ?? "Confirmed",
                OTACreatedDate = expediaReservation.CreatedDate ?? DateTime.UtcNow,
                OTALastModifiedDate = expediaReservation.LastModifiedDate
            };
        }

        // Expedia Partner Solutions API response models (temporary - gerçek API response'larına göre güncellenecek)
        private class ExpediaReservationResponse
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
