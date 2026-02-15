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
    /// Opera PMS adapter - Opera Cloud API entegrasyonu
    /// </summary>
    public class OperaPMSAdapter : BasePMSAdapter
    {
        public OperaPMSAdapter(
            PMSIntegration integration,
            IHttpClientFactory httpClientFactory,
            ILogger<OperaPMSAdapter> logger)
            : base(integration, httpClientFactory, logger)
        {
        }

        protected override void AddAuthenticationHeaders(HttpClient client)
        {
            // Opera Cloud API authentication
            // OAuth 2.0 veya API Key authentication
            if (!string.IsNullOrEmpty(_integration.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _integration.AccessToken);
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
                // Opera Cloud OAuth 2.0 token refresh
                _logger.LogInformation("Refreshing Opera access token for integration {IntegrationId}", _integration.Id);

                if (string.IsNullOrEmpty(_integration.RefreshToken) || string.IsNullOrEmpty(_integration.ApiKey))
                {
                    _logger.LogError("Missing refresh token or API key for Opera integration {IntegrationId}", _integration.Id);
                    return false;
                }

                // Create a separate client to avoid default auth headers loop
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_integration.ApiEndpoint);
                
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", _integration.RefreshToken),
                    new KeyValuePair<string, string>("client_id", _integration.ApiKey),
                    new KeyValuePair<string, string>("client_secret", _integration.ApiSecret ?? "")
                });

                // URL might be different for prod, but using suggested path
                var response = await client.PostAsync("/oauth/token", requestContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<OperaTokenResponse>(content);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        _integration.AccessToken = tokenResponse.AccessToken;
                        // Determine expiry
                        int expiresIn = tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600;
                        _integration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
                        
                        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                        {
                            _integration.RefreshToken = tokenResponse.RefreshToken;
                        }

                        _logger.LogInformation("Successfully refreshed Opera access token");
                        return true;
                    }
                }
                
                _logger.LogError("Failed to refresh Opera token. Status: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Opera access token");
                return false;
            }
        }

        public override async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Opera Cloud API health check endpoint
                var response = await CallApiAsync<object>("/api/v1/health", HttpMethod.Get);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Opera PMS Connection Test Failed for Integration {IntegrationId}", _integration.Id);
                // Re-throw so the controller can see the error
                throw new Exception($"Opera PMS Connection Failed: {ex.Message}", ex); 
            }
        }

        public override async Task<PMSGuestProfile?> GetGuestProfileAsync(string pmsGuestId)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/guests/{guestId}
                var endpoint = $"/api/v1/guests/{pmsGuestId}";
                var operaGuest = await CallApiAsync<OperaGuestResponse>(endpoint, HttpMethod.Get);
                
                if (operaGuest == null) return null;

                return MapOperaGuestToPMSGuest(operaGuest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest profile from Opera: {GuestId}", pmsGuestId);
                throw;
            }
        }

        public override async Task<List<PMSGuestProfile>> GetGuestsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/guests?startDate=...&endDate=...
                var endpoint = "/api/v1/guests";
                if (startDate.HasValue || endDate.HasValue)
                {
                    var queryParams = new List<string>();
                    if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                    if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var operaGuests = await CallApiAsync<List<OperaGuestResponse>>(endpoint, HttpMethod.Get);
                
                if (operaGuests == null) return new List<PMSGuestProfile>();

                return operaGuests.Select(MapOperaGuestToPMSGuest).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guests from Opera");
                throw;
            }
        }

        public override async Task<PMSReservation?> GetReservationAsync(string pmsReservationId)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/reservations/{reservationId}
                var endpoint = $"/api/v1/reservations/{pmsReservationId}";
                var operaReservation = await CallApiAsync<OperaReservationResponse>(endpoint, HttpMethod.Get);
                
                if (operaReservation == null) return null;

                return MapOperaReservationToPMSReservation(operaReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservation from Opera: {ReservationId}", pmsReservationId);
                throw;
            }
        }

        public override async Task<List<PMSReservation>> GetReservationsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/reservations?startDate=...&endDate=...
                var endpoint = $"/api/v1/reservations?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var operaReservations = await CallApiAsync<List<OperaReservationResponse>>(endpoint, HttpMethod.Get);
                
                if (operaReservations == null) return new List<PMSReservation>();

                return operaReservations.Select(MapOperaReservationToPMSReservation).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservations from Opera");
                throw;
            }
        }

        public override async Task<PMSRoomStatus?> GetRoomStatusAsync(string roomNumber)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/rooms/{roomNumber}/status
                var endpoint = $"/api/v1/rooms/{roomNumber}/status";
                var operaRoom = await CallApiAsync<OperaRoomResponse>(endpoint, HttpMethod.Get);
                
                if (operaRoom == null) return null;

                return MapOperaRoomToPMSRoomStatus(operaRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room status from Opera: {RoomNumber}", roomNumber);
                throw;
            }
        }

        public override async Task<List<PMSRoomStatus>> GetRoomsStatusAsync(DateTime? date = null)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/rooms/status?date=...
                var endpoint = "/api/v1/rooms/status";
                if (date.HasValue)
                {
                    endpoint += $"?date={date.Value:yyyy-MM-dd}";
                }

                var operaRooms = await CallApiAsync<List<OperaRoomResponse>>(endpoint, HttpMethod.Get);
                
                if (operaRooms == null) return new List<PMSRoomStatus>();

                return operaRooms.Select(MapOperaRoomToPMSRoomStatus).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms status from Opera");
                throw;
            }
        }

        public override async Task<PMSFolio?> GetFolioAsync(string reservationId)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/reservations/{reservationId}/folio
                var endpoint = $"/api/v1/reservations/{reservationId}/folio";
                var operaFolio = await CallApiAsync<OperaFolioResponse>(endpoint, HttpMethod.Get);
                
                if (operaFolio == null) return null;

                return MapOperaFolioToPMSFolio(operaFolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folio from Opera: {ReservationId}", reservationId);
                throw;
            }
        }

        public override async Task<List<PMSFolio>> GetFoliosAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Opera Cloud API: GET /api/v1/folios?startDate=...&endDate=...
                var endpoint = $"/api/v1/folios?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var operaFolios = await CallApiAsync<List<OperaFolioResponse>>(endpoint, HttpMethod.Get);
                
                if (operaFolios == null) return new List<PMSFolio>();

                return operaFolios.Select(MapOperaFolioToPMSFolio).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folios from Opera");
                throw;
            }
        }

        public override async Task<List<PMSRoomType>> GetRoomTypesAsync()
        {
            try
            {
                // Opera Cloud API: GET /api/v1/roomtypes
                var endpoint = "/api/v1/roomtypes";
                var operaRoomTypes = await CallApiAsync<List<OperaRoomTypeResponse>>(endpoint, HttpMethod.Get);
                
                if (operaRoomTypes == null) return new List<PMSRoomType>();

                return operaRoomTypes.Select(item => new PMSRoomType
                {
                    RoomTypeId = item.RoomTypeCode ?? string.Empty,
                    Name = item.Description ?? string.Empty,
                    BasePrice = item.MinRate ?? 0,
                    Currency = item.Currency ?? "TRY",
                    TotalInventory = item.TotalRooms ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room types from Opera");
                throw;
            }
        }

        // Mapping methods - Opera API response'larını PMS model'lerine map eder
        private PMSGuestProfile MapOperaGuestToPMSGuest(OperaGuestResponse operaGuest)
        {
            return new PMSGuestProfile
            {
                PMSGuestId = operaGuest.GuestId ?? string.Empty,
                FullName = operaGuest.FullName ?? string.Empty,
                Email = operaGuest.Email,
                PhoneNumber = operaGuest.PhoneNumber,
                Nationality = operaGuest.Nationality,
                GuestCode = operaGuest.GuestCode,
                IsVIP = operaGuest.IsVIP ?? false,
                RoomNumber = operaGuest.RoomNumber,
                CheckInDate = operaGuest.CheckInDate,
                CheckOutDate = operaGuest.CheckOutDate,
                SpecialRequests = operaGuest.SpecialRequests,
                Preferences = operaGuest.Preferences != null ? JsonSerializer.Serialize(operaGuest.Preferences) : null,
                LastUpdatedAt = operaGuest.LastUpdatedAt
            };
        }

        private PMSReservation MapOperaReservationToPMSReservation(OperaReservationResponse operaReservation)
        {
            return new PMSReservation
            {
                PMSReservationId = operaReservation.ReservationId ?? string.Empty,
                PMSGuestId = operaReservation.GuestId ?? string.Empty,
                GuestName = operaReservation.GuestName ?? string.Empty,
                GuestEmail = operaReservation.GuestEmail,
                GuestPhone = operaReservation.GuestPhone,
                CheckInDate = operaReservation.CheckInDate,
                CheckOutDate = operaReservation.CheckOutDate,
                RoomNumber = operaReservation.RoomNumber,
                RoomType = operaReservation.RoomType,
                GuestCount = operaReservation.GuestCount ?? 1,
                Status = operaReservation.Status ?? "Confirmed",
                TotalAmount = operaReservation.TotalAmount,
                Currency = operaReservation.Currency ?? "TRY",
                CreatedAt = operaReservation.CreatedAt,
                LastModifiedAt = operaReservation.LastModifiedAt
            };
        }

        private PMSRoomStatus MapOperaRoomToPMSRoomStatus(OperaRoomResponse operaRoom)
        {
            return new PMSRoomStatus
            {
                RoomNumber = operaRoom.RoomNumber ?? string.Empty,
                RoomType = operaRoom.RoomType,
                Status = operaRoom.Status ?? "Available",
                GuestName = operaRoom.GuestName,
                PMSGuestId = operaRoom.GuestId,
                CheckInDate = operaRoom.CheckInDate,
                CheckOutDate = operaRoom.CheckOutDate,
                LastUpdatedAt = operaRoom.LastUpdatedAt
            };
        }

        private PMSFolio MapOperaFolioToPMSFolio(OperaFolioResponse operaFolio)
        {
            return new PMSFolio
            {
                FolioId = operaFolio.FolioId ?? string.Empty,
                ReservationId = operaFolio.ReservationId ?? string.Empty,
                GuestName = operaFolio.GuestName ?? string.Empty,
                TotalAmount = operaFolio.TotalAmount ?? 0,
                PaidAmount = operaFolio.PaidAmount,
                Balance = operaFolio.Balance,
                Currency = operaFolio.Currency ?? "TRY",
                Status = operaFolio.Status ?? "Open",
                FolioDate = operaFolio.FolioDate,
                Items = operaFolio.Items?.Select(item => new PMSFolioItem
                {
                    Description = item.Description ?? string.Empty,
                    Amount = item.Amount ?? 0,
                    Category = item.Category,
                    TransactionDate = item.TransactionDate
                }).ToList() ?? new List<PMSFolioItem>()
            };
        }

        // Opera API response models (temporary - gerçek API response'larına göre güncellenecek)
        private class OperaGuestResponse
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

        private class OperaReservationResponse
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

        private class OperaRoomResponse
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

        private class OperaFolioResponse
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
            public List<OperaFolioItemResponse>? Items { get; set; }
        }

        private class OperaFolioItemResponse
        {
            public string? Description { get; set; }
            public decimal? Amount { get; set; }
            public string? Category { get; set; }
            public DateTime? TransactionDate { get; set; }
        }
        private class OperaRoomTypeResponse
        {
            public string? RoomTypeCode { get; set; }
            public string? Description { get; set; }
            public decimal? MinRate { get; set; }
            public string? Currency { get; set; }
            public int? TotalRooms { get; set; }
        }

        private class OperaTokenResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("token_type")]
            public string? TokenType { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }
        }
    }
}
