// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// Elektraweb PMS adapter - Elektraweb API entegrasyonu
    /// </summary>
    public class ElektrawebPMSAdapter : BasePMSAdapter
    {
        public ElektrawebPMSAdapter(
            PMSIntegration integration,
            IHttpClientFactory httpClientFactory,
            ILogger<ElektrawebPMSAdapter> logger)
            : base(integration, httpClientFactory, logger)
        {
        }

        protected override void AddAuthenticationHeaders(HttpClient client)
        {
            // Elektraweb API authentication
            // Elektraweb genellikle API Key veya Basic Auth kullanır
            if (!string.IsNullOrEmpty(_integration.ApiKey) && !string.IsNullOrEmpty(_integration.ApiSecret))
            {
                // Basic Auth veya custom header
                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{_integration.ApiKey}:{_integration.ApiSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }
            else if (!string.IsNullOrEmpty(_integration.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", _integration.ApiKey);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public override async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                // Elektraweb token refresh (eğer OAuth kullanıyorsa)
                _logger.LogInformation("Refreshing Elektraweb access token for integration {IntegrationId}", _integration.Id);
                
                // TODO: Elektraweb token refresh implementation
                // Elektraweb API dokümantasyonuna göre implement edilecek
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Elektraweb access token");
                return false;
            }
        }

        public override async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Elektraweb API health check endpoint
                var response = await CallApiAsync<object>("/api/health", HttpMethod.Get);
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        public override async Task<PMSGuestProfile?> GetGuestProfileAsync(string pmsGuestId)
        {
            try
            {
                // Elektraweb API: GET /api/guests/{guestId}
                var endpoint = $"/api/guests/{pmsGuestId}";
                var elektrawebGuest = await CallApiAsync<ElektrawebGuestResponse>(endpoint, HttpMethod.Get);
                
                if (elektrawebGuest == null) return null;

                return MapElektrawebGuestToPMSGuest(elektrawebGuest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest profile from Elektraweb: {GuestId}", pmsGuestId);
                throw;
            }
        }

        public override async Task<List<PMSGuestProfile>> GetGuestsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Elektraweb API: GET /api/guests?startDate=...&endDate=...
                var endpoint = "/api/guests";
                if (startDate.HasValue || endDate.HasValue)
                {
                    var queryParams = new List<string>();
                    if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                    if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var elektrawebGuests = await CallApiAsync<List<ElektrawebGuestResponse>>(endpoint, HttpMethod.Get);
                
                if (elektrawebGuests == null) return new List<PMSGuestProfile>();

                return elektrawebGuests.Select(MapElektrawebGuestToPMSGuest).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guests from Elektraweb");
                throw;
            }
        }

        public override async Task<PMSReservation?> GetReservationAsync(string pmsReservationId)
        {
            try
            {
                // Elektraweb API: GET /api/reservations/{reservationId}
                var endpoint = $"/api/reservations/{pmsReservationId}";
                var elektrawebReservation = await CallApiAsync<ElektrawebReservationResponse>(endpoint, HttpMethod.Get);
                
                if (elektrawebReservation == null) return null;

                return MapElektrawebReservationToPMSReservation(elektrawebReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservation from Elektraweb: {ReservationId}", pmsReservationId);
                throw;
            }
        }

        public override async Task<List<PMSReservation>> GetReservationsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Elektraweb API: GET /api/reservations?startDate=...&endDate=...
                var endpoint = $"/api/reservations?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var elektrawebReservations = await CallApiAsync<List<ElektrawebReservationResponse>>(endpoint, HttpMethod.Get);
                
                if (elektrawebReservations == null) return new List<PMSReservation>();

                return elektrawebReservations.Select(MapElektrawebReservationToPMSReservation).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservations from Elektraweb");
                throw;
            }
        }

        public override async Task<PMSRoomStatus?> GetRoomStatusAsync(string roomNumber)
        {
            try
            {
                // Elektraweb API: GET /api/rooms/{roomNumber}/status
                var endpoint = $"/api/rooms/{roomNumber}/status";
                var elektrawebRoom = await CallApiAsync<ElektrawebRoomResponse>(endpoint, HttpMethod.Get);
                
                if (elektrawebRoom == null) return null;

                return MapElektrawebRoomToPMSRoomStatus(elektrawebRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room status from Elektraweb: {RoomNumber}", roomNumber);
                throw;
            }
        }

        public override async Task<List<PMSRoomStatus>> GetRoomsStatusAsync(DateTime? date = null)
        {
            try
            {
                // Elektraweb API: GET /api/rooms/status?date=...
                var endpoint = "/api/rooms/status";
                if (date.HasValue)
                {
                    endpoint += $"?date={date.Value:yyyy-MM-dd}";
                }

                var elektrawebRooms = await CallApiAsync<List<ElektrawebRoomResponse>>(endpoint, HttpMethod.Get);
                
                if (elektrawebRooms == null) return new List<PMSRoomStatus>();

                return elektrawebRooms.Select(MapElektrawebRoomToPMSRoomStatus).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms status from Elektraweb");
                throw;
            }
        }

        public override async Task<PMSFolio?> GetFolioAsync(string reservationId)
        {
            try
            {
                // Elektraweb API: GET /api/reservations/{reservationId}/folio
                var endpoint = $"/api/reservations/{reservationId}/folio";
                var elektrawebFolio = await CallApiAsync<ElektrawebFolioResponse>(endpoint, HttpMethod.Get);
                
                if (elektrawebFolio == null) return null;

                return MapElektrawebFolioToPMSFolio(elektrawebFolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folio from Elektraweb: {ReservationId}", reservationId);
                throw;
            }
        }

        public override async Task<List<PMSFolio>> GetFoliosAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Elektraweb API: GET /api/folios?startDate=...&endDate=...
                var endpoint = $"/api/folios?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var elektrawebFolios = await CallApiAsync<List<ElektrawebFolioResponse>>(endpoint, HttpMethod.Get);
                
                if (elektrawebFolios == null) return new List<PMSFolio>();

                return elektrawebFolios.Select(MapElektrawebFolioToPMSFolio).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folios from Elektraweb");
                throw;
            }
        }

        public override Task<List<PMSRoomType>> GetRoomTypesAsync()
        {
            // TODO: Implement actual API call to Elektraweb
            _logger.LogWarning("GetRoomTypesAsync not implemented for Elektraweb, returning empty list.");
            return Task.FromResult(new List<PMSRoomType>());
        }

        // Mapping methods - Elektraweb API response'larını PMS model'lerine map eder
        private PMSGuestProfile MapElektrawebGuestToPMSGuest(ElektrawebGuestResponse elektrawebGuest)
        {
            return new PMSGuestProfile
            {
                PMSGuestId = elektrawebGuest.GuestId ?? string.Empty,
                FullName = elektrawebGuest.FullName ?? string.Empty,
                Email = elektrawebGuest.Email,
                PhoneNumber = elektrawebGuest.PhoneNumber,
                Nationality = elektrawebGuest.Nationality,
                GuestCode = elektrawebGuest.GuestCode,
                IsVIP = elektrawebGuest.IsVIP ?? false,
                RoomNumber = elektrawebGuest.RoomNumber,
                CheckInDate = elektrawebGuest.CheckInDate,
                CheckOutDate = elektrawebGuest.CheckOutDate,
                SpecialRequests = elektrawebGuest.SpecialRequests,
                Preferences = elektrawebGuest.Preferences != null ? JsonSerializer.Serialize(elektrawebGuest.Preferences) : null,
                LastUpdatedAt = elektrawebGuest.LastUpdatedAt
            };
        }

        private PMSReservation MapElektrawebReservationToPMSReservation(ElektrawebReservationResponse elektrawebReservation)
        {
            return new PMSReservation
            {
                PMSReservationId = elektrawebReservation.ReservationId ?? string.Empty,
                PMSGuestId = elektrawebReservation.GuestId ?? string.Empty,
                GuestName = elektrawebReservation.GuestName ?? string.Empty,
                GuestEmail = elektrawebReservation.GuestEmail,
                GuestPhone = elektrawebReservation.GuestPhone,
                CheckInDate = elektrawebReservation.CheckInDate,
                CheckOutDate = elektrawebReservation.CheckOutDate,
                RoomNumber = elektrawebReservation.RoomNumber,
                RoomType = elektrawebReservation.RoomType,
                GuestCount = elektrawebReservation.GuestCount ?? 1,
                Status = elektrawebReservation.Status ?? "Confirmed",
                TotalAmount = elektrawebReservation.TotalAmount,
                Currency = elektrawebReservation.Currency ?? "TRY",
                CreatedAt = elektrawebReservation.CreatedAt,
                LastModifiedAt = elektrawebReservation.LastModifiedAt
            };
        }

        private PMSRoomStatus MapElektrawebRoomToPMSRoomStatus(ElektrawebRoomResponse elektrawebRoom)
        {
            return new PMSRoomStatus
            {
                RoomNumber = elektrawebRoom.RoomNumber ?? string.Empty,
                RoomType = elektrawebRoom.RoomType,
                Status = elektrawebRoom.Status ?? "Available",
                GuestName = elektrawebRoom.GuestName,
                PMSGuestId = elektrawebRoom.GuestId,
                CheckInDate = elektrawebRoom.CheckInDate,
                CheckOutDate = elektrawebRoom.CheckOutDate,
                LastUpdatedAt = elektrawebRoom.LastUpdatedAt
            };
        }

        private PMSFolio MapElektrawebFolioToPMSFolio(ElektrawebFolioResponse elektrawebFolio)
        {
            return new PMSFolio
            {
                FolioId = elektrawebFolio.FolioId ?? string.Empty,
                ReservationId = elektrawebFolio.ReservationId ?? string.Empty,
                GuestName = elektrawebFolio.GuestName ?? string.Empty,
                TotalAmount = elektrawebFolio.TotalAmount ?? 0,
                PaidAmount = elektrawebFolio.PaidAmount,
                Balance = elektrawebFolio.Balance,
                Currency = elektrawebFolio.Currency ?? "TRY",
                Status = elektrawebFolio.Status ?? "Open",
                FolioDate = elektrawebFolio.FolioDate,
                Items = elektrawebFolio.Items?.Select(item => new PMSFolioItem
                {
                    Description = item.Description ?? string.Empty,
                    Amount = item.Amount ?? 0,
                    Category = item.Category,
                    TransactionDate = item.TransactionDate
                }).ToList() ?? new List<PMSFolioItem>()
            };
        }

        // Elektraweb API response models (temporary - gerçek API response'larına göre güncellenecek)
        private class ElektrawebGuestResponse
        {
            public string? GuestId { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Nationality { get; set; }
            public string? GuestCode { get; set; }
            public bool? IsVIP { get; set; }
            public string? RoomNumber { get; set; }
            public DateTime? CheckInDate { get; set; }
            public DateTime? CheckOutDate { get; set; }
            public string? SpecialRequests { get; set; }
            public object? Preferences { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
        }

        private class ElektrawebReservationResponse
        {
            public string? ReservationId { get; set; }
            public string? GuestId { get; set; }
            public string? GuestName { get; set; }
            public string? GuestEmail { get; set; }
            public string? GuestPhone { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public string? RoomNumber { get; set; }
            public string? RoomType { get; set; }
            public int? GuestCount { get; set; }
            public string? Status { get; set; }
            public decimal? TotalAmount { get; set; }
            public string? Currency { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? LastModifiedAt { get; set; }
        }

        private class ElektrawebRoomResponse
        {
            public string? RoomNumber { get; set; }
            public string? RoomType { get; set; }
            public string? Status { get; set; }
            public string? GuestName { get; set; }
            public string? GuestId { get; set; }
            public DateTime? CheckInDate { get; set; }
            public DateTime? CheckOutDate { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
        }

        private class ElektrawebFolioResponse
        {
            public string? FolioId { get; set; }
            public string? ReservationId { get; set; }
            public string? GuestName { get; set; }
            public decimal? TotalAmount { get; set; }
            public decimal? PaidAmount { get; set; }
            public decimal? Balance { get; set; }
            public string? Currency { get; set; }
            public string? Status { get; set; }
            public DateTime? FolioDate { get; set; }
            public List<ElektrawebFolioItemResponse>? Items { get; set; }
        }

        private class ElektrawebFolioItemResponse
        {
            public string? Description { get; set; }
            public decimal? Amount { get; set; }
            public string? Category { get; set; }
            public DateTime? TransactionDate { get; set; }
        }
    }
}
