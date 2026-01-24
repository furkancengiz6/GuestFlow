// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// Base OTA adapter - tüm OTA adapter'ları için ortak fonksiyonellik
    /// </summary>
    public abstract class BaseOTAAdapter
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger _logger;
        protected readonly OTAIntegration _integration;

        protected BaseOTAAdapter(
            OTAIntegration integration,
            IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            _integration = integration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// HTTP client oluştur ve authentication header'ları ekle
        /// </summary>
        protected virtual HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_integration.ApiEndpoint);
            client.Timeout = TimeSpan.FromSeconds(30);

            // Authentication header'ı ekle (provider'a göre override edilebilir)
            AddAuthenticationHeaders(client);

            return client;
        }

        /// <summary>
        /// Authentication header'ları ekle (provider'a göre implement edilir)
        /// </summary>
        protected abstract void AddAuthenticationHeaders(HttpClient client);

        /// <summary>
        /// Access token'ı yenile (OAuth 2.0 için)
        /// </summary>
        public abstract Task<bool> RefreshAccessTokenAsync();

        /// <summary>
        /// API çağrısı yap ve response'u parse et (retry logic ile)
        /// </summary>
        protected async Task<T?> CallApiAsync<T>(string endpoint, HttpMethod method, object? body = null, int maxRetries = 3)
        {
            int retryCount = 0;
            Exception? lastException = null;

            while (retryCount <= maxRetries)
            {
                try
                {
                    using var client = CreateHttpClient();
                    var request = new HttpRequestMessage(method, endpoint);

                    if (body != null)
                    {
                        var json = JsonSerializer.Serialize(body);
                        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    }

                    var response = await client.SendAsync(request);

                    // 401 Unauthorized - Token yenile ve tekrar dene
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retryCount < maxRetries)
                    {
                        _logger.LogWarning("Unauthorized response, attempting to refresh token. Retry: {RetryCount}", retryCount);
                        var tokenRefreshed = await RefreshAccessTokenAsync();
                        if (tokenRefreshed)
                        {
                            retryCount++;
                            await Task.Delay(1000 * retryCount); // Exponential backoff
                            continue;
                        }
                    }

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (HttpRequestException ex) when (retryCount < maxRetries)
                {
                    lastException = ex;
                    retryCount++;
                    
                    // Exponential backoff: 1s, 2s, 4s
                    var delay = TimeSpan.FromMilliseconds(1000 * Math.Pow(2, retryCount - 1));
                    _logger.LogWarning(ex, "OTA API call failed, retrying in {Delay}ms. Attempt: {RetryCount}/{MaxRetries}", 
                        delay.TotalMilliseconds, retryCount, maxRetries);
                    
                    await Task.Delay(delay);
                }
                catch (TaskCanceledException ex) when (retryCount < maxRetries)
                {
                    lastException = ex;
                    retryCount++;
                    
                    var delay = TimeSpan.FromMilliseconds(1000 * Math.Pow(2, retryCount - 1));
                    _logger.LogWarning(ex, "OTA API call timeout, retrying in {Delay}ms. Attempt: {RetryCount}/{MaxRetries}", 
                        delay.TotalMilliseconds, retryCount, maxRetries);
                    
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OTA API call failed: {Endpoint}, Provider: {Provider}", endpoint, _integration.ProviderName);
                    throw;
                }
            }

            // Tüm retry'lar başarısız oldu
            _logger.LogError(lastException, "OTA API call failed after {MaxRetries} retries: {Endpoint}, Provider: {Provider}", 
                maxRetries, endpoint, _integration.ProviderName);
            throw lastException ?? new Exception($"OTA API call failed after {maxRetries} retries");
        }

        /// <summary>
        /// Test connection - provider'a göre implement edilir
        /// </summary>
        public abstract Task<bool> TestConnectionAsync();

        // Abstract methods - her provider kendi implementasyonunu yapacak
        public abstract Task<List<OTAReservationDto>> GetReservationsAsync(DateTime startDate, DateTime endDate);
        public abstract Task<OTAReservationDto?> GetReservationAsync(string otaReservationId);
        public abstract Task<bool> UpdateAvailabilityAsync(string roomTypeId, DateTime date, bool isAvailable);
        public abstract Task<bool> UpdateRatesAsync(string roomTypeId, DateTime date, decimal price, string currency);
        public abstract Task<bool> ProcessWebhookAsync(string payload, string? signature = null);
    }

    /// <summary>
    /// OTA Reservation DTO (generic model)
    /// </summary>
    public class OTAReservationDto
    {
        public string OTAReservationId { get; set; } = string.Empty;
        public string OTAHotelId { get; set; } = string.Empty;
        public string OTARoomTypeId { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "TRY";
        public string GuestName { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        public string Status { get; set; } = "Confirmed";
        public DateTime OTACreatedDate { get; set; }
        public DateTime? OTALastModifiedDate { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
