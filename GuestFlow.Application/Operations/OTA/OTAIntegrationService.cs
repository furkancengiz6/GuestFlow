using GuestFlow.Application.Models.Requests.OTA;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace GuestFlow.Application.Operations.OTA
{
    public class OTAIntegrationService : IOTAIntegrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly ILogger<OTAIntegrationService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public OTAIntegrationService(
            IUnitOfWork unitOfWork, 
            IHttpClientFactory httpClientFactory,
            IPMSIntegrationService pmsIntegrationService,
            ILogger<OTAIntegrationService> logger,
            ILoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _pmsIntegrationService = pmsIntegrationService;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// OTA adapter oluştur (factory pattern)
        /// </summary>
        private BaseOTAAdapter CreateAdapter(OTAIntegration integration)
        {
            var providerCode = integration.ProviderCode.ToUpperInvariant();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            return providerCode switch
            {
                "BKG" or "BOOKING" or "BOOKINGCOM" => new BookingComAdapter(integration, _httpClientFactory,
                    loggerFactory.CreateLogger<BookingComAdapter>()),
                "EXP" or "EXPEDIA" => new ExpediaAdapter(integration, _httpClientFactory,
                    loggerFactory.CreateLogger<ExpediaAdapter>()),
                _ => throw new NotSupportedException($"OTA provider '{integration.ProviderCode}' is not supported")
            };
        }

        public async Task<ApiResponse<OTAIntegration>> CreateOTAIntegrationAsync(CreateOTAIntegrationRequest request)
        {
            try
            {
                var integration = new OTAIntegration
                {
                    ProviderName = request.ProviderName,
                    ProviderCode = request.ProviderCode,
                    ApiEndpoint = request.ApiEndpoint,
                    ApiKey = request.ApiKey,
                    ApiSecret = request.ApiSecret,
                    WebhookUrl = request.WebhookUrl,
                    IsActive = request.IsActive
                };

                await _unitOfWork.OTAIntegrations.AddAsync(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<OTAIntegration>.SuccessResponse(integration, "OTA integration created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OTAIntegration>.Fail($"Failed to create OTA integration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OTAIntegration>>> GetAllOTAIntegrationsAsync()
        {
            try
            {
                var integrations = await _unitOfWork.OTAIntegrations.GetAll(i => i.IsActive).ToListAsync();
                return ApiResponse<List<OTAIntegration>>.SuccessResponse(integrations);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<OTAIntegration>>.Fail($"Failed to get OTA integrations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> TestOTAConnectionAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.OTAIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                var adapter = CreateAdapter(integration);
                var isConnected = await adapter.TestConnectionAsync();

                // Update last sync info
                integration.LastSyncDate = DateTime.UtcNow;
                integration.LastSyncStatus = isConnected ? "Success" : "Failed";
                if (!isConnected)
                {
                    integration.SyncErrorMessage = "Connection test failed";
                }

                _unitOfWork.OTAIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(isConnected,
                    isConnected ? "Connection successful" : "Connection failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for OTA integration: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Connection test failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncReservationsAsync(int integrationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var integration = await _unitOfWork.OTAIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                var adapter = CreateAdapter(integration);
                
                // OTA'dan rezervasyonları getir
                var otaReservations = await adapter.GetReservationsAsync(startDate, endDate);
                
                int processed = 0, succeeded = 0, failed = 0;

                foreach (var otaReservation in otaReservations)
                {
                    try
                    {
                        processed++;
                        await SyncOTAReservationToGuestFlowAsync(integrationId, otaReservation);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(ex, "Failed to sync OTA reservation: {OTAReservationId}", otaReservation.OTAReservationId);
                    }
                }

                // Update last sync info
                integration.LastSyncDate = DateTime.UtcNow;
                integration.LastSyncStatus = failed == 0 ? "Success" : "PartialSuccess";
                integration.SyncErrorMessage = failed > 0 ? $"{failed} reservations failed to sync" : null;

                _unitOfWork.OTAIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, 
                    $"Synced {succeeded} reservations successfully. {failed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync failed for OTA integration: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Sync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// OTA rezervasyonunu GuestFlow'a senkronize et ve PMS'e gönder
        /// Conflict kontrolü ve duplicate önleme ile
        /// </summary>
        private async Task SyncOTAReservationToGuestFlowAsync(int integrationId, OTAReservationDto otaReservation)
        {
            // Mevcut OTA rezervasyonunu kontrol et
            var existingOTAReservation = await _unitOfWork.OTAReservations
                .GetAll(r => r.OTAIntegrationId == integrationId && r.OTAReservationId == otaReservation.OTAReservationId)
                .FirstOrDefaultAsync();

            if (existingOTAReservation == null)
            {
                // Yeni OTA rezervasyonu oluştur
                var newOTAReservation = new OTAReservation
                {
                    OTAIntegrationId = integrationId,
                    OTAReservationId = otaReservation.OTAReservationId,
                    OTAHotelId = otaReservation.OTAHotelId,
                    OTARoomTypeId = otaReservation.OTARoomTypeId,
                    CheckInDate = otaReservation.CheckInDate,
                    CheckOutDate = otaReservation.CheckOutDate,
                    GuestCount = otaReservation.GuestCount,
                    TotalPrice = otaReservation.TotalPrice,
                    Currency = otaReservation.Currency,
                    GuestName = otaReservation.GuestName,
                    GuestEmail = otaReservation.GuestEmail,
                    GuestPhone = otaReservation.GuestPhone,
                    Status = otaReservation.Status,
                    OTACreatedDate = otaReservation.OTACreatedDate,
                    OTALastModifiedDate = otaReservation.OTALastModifiedDate
                };

                await _unitOfWork.OTAReservations.AddAsync(newOTAReservation);
                await _unitOfWork.CommitAsync();

                // PMS'e gönder (aktif PMS entegrasyonları varsa)
                await SendOTAReservationToPMSAsync(integrationId, newOTAReservation);
            }
            else
            {
                // Mevcut rezervasyonu güncelle
                existingOTAReservation.CheckInDate = otaReservation.CheckInDate;
                existingOTAReservation.CheckOutDate = otaReservation.CheckOutDate;
                existingOTAReservation.GuestCount = otaReservation.GuestCount;
                existingOTAReservation.TotalPrice = otaReservation.TotalPrice;
                existingOTAReservation.Status = otaReservation.Status;
                existingOTAReservation.OTALastModifiedDate = otaReservation.OTALastModifiedDate;

                _unitOfWork.OTAReservations.Update(existingOTAReservation);
                await _unitOfWork.CommitAsync();

                // PMS'e güncelleme gönder
                await SendOTAReservationToPMSAsync(integrationId, existingOTAReservation);
            }
        }

        /// <summary>
        /// OTA rezervasyonunu aktif PMS entegrasyonlarına gönder
        /// </summary>
        private async Task SendOTAReservationToPMSAsync(int integrationId, OTAReservation otaReservation)
        {
            try
            {
                // Aktif PMS entegrasyonlarını bul
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var pmsIntegration in activePMSIntegrations)
                {
                    try
                    {
                        // OTA rezervasyonunu PMS reservation formatına çevir
                        // Not: PMS'de reservation oluşturma API'si yoksa, sadece guest oluşturulabilir
                        // Bu durumda guest oluştur ve mapping yap
                        
                        // Guest oluştur veya bul
                        var guest = await _unitOfWork.Guests
                            .GetAll(g => g.Email == otaReservation.GuestEmail && !g.IsDeleted)
                            .FirstOrDefaultAsync();

                        if (guest == null)
                        {
                            // Yeni guest oluştur
                            var guestCode = await GenerateGuestCodeAsync();
                            guest = new GuestEntity
                            {
                                GuestCode = guestCode,
                                FullName = otaReservation.GuestName,
                                Email = otaReservation.GuestEmail,
                                PhoneNumber = otaReservation.GuestPhone,
                                CheckInDate = otaReservation.CheckInDate,
                                CheckOutDate = otaReservation.CheckOutDate,
                                RoomNumber = null // OTA'dan room number gelmez, PMS'den gelecek
                            };

                            await _unitOfWork.Guests.AddAsync(guest);
                            await _unitOfWork.CommitAsync();
                        }
                        else
                        {
                            // Mevcut guest'i güncelle
                            guest.CheckInDate = otaReservation.CheckInDate;
                            guest.CheckOutDate = otaReservation.CheckOutDate;
                            _unitOfWork.Guests.Update(guest);
                            await _unitOfWork.CommitAsync();
                        }

                        // OTA rezervasyonunu guest ile eşleştir
                        otaReservation.GuestFlowReservationId = null; // GuestFlow'da reservation entity yok, sadece guest var
                        _unitOfWork.OTAReservations.Update(otaReservation);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation("OTA reservation {OTAReservationId} synced to PMS {PMSProvider} via guest {GuestId}",
                            otaReservation.OTAReservationId, pmsIntegration.ProviderName, guest.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send OTA reservation to PMS {PMSProvider}: {OTAReservationId}",
                            pmsIntegration.ProviderName, otaReservation.OTAReservationId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTA reservation to PMS: {OTAReservationId}", otaReservation.OTAReservationId);
            }
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

        public async Task<ApiResponse<bool>> UpdateRoomPricesAsync(int integrationId, int hotelId, List<PriceUpdateRequest> prices)
        {
            try
            {
                var integration = await _unitOfWork.OTAIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                var adapter = CreateAdapter(integration);
                int succeeded = 0, failed = 0;

                // Her fiyat güncellemesini OTA'ya gönder
                foreach (var price in prices)
                {
                    try
                    {
                        // Availability güncelle
                        await adapter.UpdateAvailabilityAsync(price.RoomTypeId, price.Date, price.IsAvailable);

                        // Rate güncelle
                        await adapter.UpdateRatesAsync(price.RoomTypeId, price.Date, price.Price, price.Currency);

                        // Price update record oluştur
                        var priceUpdate = new OTAPriceUpdate
                        {
                            OTAIntegrationId = integrationId,
                            HotelId = hotelId,
                            OTARoomTypeId = price.RoomTypeId,
                            Date = price.Date,
                            Price = price.Price,
                            Currency = price.Currency,
                            IsAvailable = price.IsAvailable,
                            UpdateStatus = "Sent",
                            SentAt = DateTime.UtcNow
                        };

                        await _unitOfWork.OTAPriceUpdates.AddAsync(priceUpdate);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(ex, "Failed to update price for RoomTypeId={RoomTypeId}, Date={Date}",
                            price.RoomTypeId, price.Date);

                        // Failed record oluştur
                        var priceUpdate = new OTAPriceUpdate
                        {
                            OTAIntegrationId = integrationId,
                            HotelId = hotelId,
                            OTARoomTypeId = price.RoomTypeId,
                            Date = price.Date,
                            Price = price.Price,
                            Currency = price.Currency,
                            IsAvailable = price.IsAvailable,
                            UpdateStatus = "Failed",
                            ErrorMessage = ex.Message
                        };

                        await _unitOfWork.OTAPriceUpdates.AddAsync(priceUpdate);
                    }
                }

                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, 
                    $"Updated {succeeded} prices successfully. {failed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Price update failed for OTA integration: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Price update failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OTAReservation>>> GetPendingReservationsAsync(int integrationId)
        {
            try
            {
                var reservations = await _unitOfWork.OTAReservations
                    .GetAll(r => r.OTAIntegrationId == integrationId &&
                                (r.Status == "Pending" || r.Status == "Modified") &&
                                r.GuestFlowReservationId == null)
                    .ToListAsync();

                return ApiResponse<List<OTAReservation>>.SuccessResponse(reservations);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<OTAReservation>>.Fail($"Failed to get pending reservations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ProcessWebhookAsync(
            string providerCode, 
            string payload, 
            string? signature = null,
            string? idempotencyKey = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            try
            {
                // Find integration by provider code
                var integration = await _unitOfWork.OTAIntegrations
                    .GetAll(i => i.ProviderCode == providerCode && i.IsActive)
                    .FirstOrDefaultAsync();

                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                // Check rate limit
                if (!CheckRateLimit(providerCode))
                {
                    return ApiResponse<bool>.Fail("Rate limit exceeded. Please try again later.");
                }

                // Check circuit breaker
                if (!CheckCircuitBreaker(providerCode))
                {
                    return ApiResponse<bool>.Fail("Service temporarily unavailable. Circuit breaker is open.");
                }

                // Generate idempotency key if not provided (from payload hash)
                if (string.IsNullOrEmpty(idempotencyKey))
                {
                    idempotencyKey = GenerateIdempotencyKey(providerCode, payload);
                }

                // Check if webhook with this idempotency key was already processed
                var existingLog = await _unitOfWork.OTAWebhookLogs
                    .GetAll(w => w.IdempotencyKey == idempotencyKey && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingLog != null)
                {
                    // If already successfully processed, return success
                    if (existingLog.Status == "Success")
                    {
                        _logger.LogInformation("Webhook already processed (idempotency key: {IdempotencyKey})", idempotencyKey);
                        return ApiResponse<bool>.SuccessResponse(true, "Webhook already processed successfully");
                    }

                    // If failed and retry count exceeded, return error
                    if (existingLog.RetryCount >= existingLog.MaxRetries)
                    {
                        _logger.LogWarning("Webhook retry limit exceeded (idempotency key: {IdempotencyKey}, retries: {RetryCount})", 
                            idempotencyKey, existingLog.RetryCount);
                        return ApiResponse<bool>.Fail("Webhook retry limit exceeded");
                    }

                    // If pending retry, check if it's time to retry
                    if (existingLog.Status == "Failed" && existingLog.NextRetryAt.HasValue)
                    {
                        if (DateTime.UtcNow < existingLog.NextRetryAt.Value)
                        {
                            _logger.LogInformation("Webhook retry not yet due (idempotency key: {IdempotencyKey}, next retry: {NextRetryAt})", 
                                idempotencyKey, existingLog.NextRetryAt);
                            return ApiResponse<bool>.Fail("Webhook retry not yet due");
                        }
                    }
                }

                // Create webhook handler
                var webhookHandler = new OTAWebhookHandler(
                    _unitOfWork,
                    this,
                    _pmsIntegrationService,
                    _loggerFactory.CreateLogger<OTAWebhookHandler>());

                // Create or update webhook log
                var webhookLog = existingLog ?? new OTAWebhookLog
                {
                    OTAIntegrationId = integration.Id,
                    ProviderCode = providerCode,
                    IdempotencyKey = idempotencyKey,
                    Payload = payload,
                    Signature = signature,
                    Status = "Pending",
                    ProcessedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    MaxRetries = 3
                };

                if (existingLog != null)
                {
                    webhookLog.RetryCount++;
                    webhookLog.LastRetryAt = DateTime.UtcNow;
                    webhookLog.Status = "Processing";
                    _unitOfWork.OTAWebhookLogs.Update(webhookLog);
                }
                else
                {
                    webhookLog.Status = "Processing";
                    await _unitOfWork.OTAWebhookLogs.AddAsync(webhookLog);
                }

                await _unitOfWork.CommitAsync();

                try
                {
                    // Webhook signature validation
                    if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(integration.ApiSecret))
                    {
                        var isValid = webhookHandler.ValidateWebhookSignature(payload, signature, integration.ApiSecret, providerCode);
                        if (!isValid)
                        {
                            _logger.LogWarning("Invalid webhook signature for provider: {ProviderCode}", providerCode);
                            webhookLog.Status = "Failed";
                            webhookLog.ErrorMessage = "Invalid webhook signature";
                            webhookLog.CompletedAt = DateTime.UtcNow;
                            webhookLog.NextRetryAt = CalculateNextRetryTime(webhookLog.RetryCount);
                            _unitOfWork.OTAWebhookLogs.Update(webhookLog);
                            await _unitOfWork.CommitAsync();
                            return ApiResponse<bool>.Fail("Invalid webhook signature");
                        }
                    }

                    // Parse webhook payload
                    var webhookEvent = webhookHandler.ParseWebhookPayload(payload, providerCode);
                    if (webhookEvent == null)
                    {
                        _logger.LogWarning("Failed to parse webhook payload for provider: {ProviderCode}", providerCode);
                        webhookLog.Status = "Failed";
                        webhookLog.ErrorMessage = "Failed to parse webhook payload";
                        webhookLog.CompletedAt = DateTime.UtcNow;
                        webhookLog.NextRetryAt = CalculateNextRetryTime(webhookLog.RetryCount);
                        _unitOfWork.OTAWebhookLogs.Update(webhookLog);
                        await _unitOfWork.CommitAsync();
                        return ApiResponse<bool>.Fail("Failed to parse webhook payload");
                    }

                    webhookLog.EventType = webhookEvent.EventType ?? "";
                    webhookLog.ReservationId = webhookEvent.ReservationId;

                    // Process webhook event
                    var result = await webhookHandler.ProcessWebhookEventAsync(integration.Id, webhookEvent);

                    // Update circuit breaker state
                    if (result)
                    {
                        RecordCircuitBreakerSuccess(providerCode);
                    }
                    else
                    {
                        RecordCircuitBreakerFailure(providerCode);
                    }

                    // Update webhook log
                    webhookLog.Status = result ? "Success" : "Failed";
                    webhookLog.CompletedAt = DateTime.UtcNow;
                    if (!result)
                    {
                        webhookLog.ErrorMessage = "Webhook processing failed";
                        webhookLog.NextRetryAt = CalculateNextRetryTime(webhookLog.RetryCount);
                        
                        // Move to dead-letter queue if retry limit exceeded
                        if (webhookLog.RetryCount >= webhookLog.MaxRetries)
                        {
                            webhookLog.IsDeadLetter = true;
                            webhookLog.DeadLetterAt = DateTime.UtcNow;
                        }
                    }
                    _unitOfWork.OTAWebhookLogs.Update(webhookLog);

                    // Update last sync info
                    integration.LastSyncDate = DateTime.UtcNow;
                    integration.LastSyncStatus = result ? "Success" : "Failed";
                    _unitOfWork.OTAIntegrations.Update(integration);
                    await _unitOfWork.CommitAsync();

                    return ApiResponse<bool>.SuccessResponse(result, 
                        result ? "Webhook processed successfully" : "Webhook processing failed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Webhook processing failed for provider: {ProviderCode}", providerCode);
                    
                    // Update webhook log with error
                    webhookLog.Status = "Failed";
                    webhookLog.ErrorMessage = ex.Message;
                    webhookLog.ErrorDetails = ex.ToString();
                    webhookLog.CompletedAt = DateTime.UtcNow;
                    webhookLog.NextRetryAt = CalculateNextRetryTime(webhookLog.RetryCount);
                    
                    // Move to dead-letter queue if retry limit exceeded
                    if (webhookLog.RetryCount >= webhookLog.MaxRetries)
                    {
                        webhookLog.IsDeadLetter = true;
                        webhookLog.DeadLetterAt = DateTime.UtcNow;
                    }
                    
                    _unitOfWork.OTAWebhookLogs.Update(webhookLog);
                    await _unitOfWork.CommitAsync();
                    
                    return ApiResponse<bool>.Fail($"Webhook processing failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed for provider: {ProviderCode}", providerCode);
                return ApiResponse<bool>.Fail($"Webhook processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate idempotency key from provider code and payload
        /// </summary>
        private string GenerateIdempotencyKey(string providerCode, string payload)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{providerCode}:{payload}"));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Calculate next retry time with exponential backoff
        /// </summary>
        private DateTime? CalculateNextRetryTime(int retryCount)
        {
            if (retryCount <= 0)
                return null;

            // Exponential backoff: 1min, 5min, 15min, 30min
            var backoffMinutes = retryCount switch
            {
                1 => 1,
                2 => 5,
                3 => 15,
                _ => 30
            };

            return DateTime.UtcNow.AddMinutes(backoffMinutes);
        }

        public async Task<ApiResponse<object>> GetDeadLetterWebhooksAsync(string? providerCode = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                IQueryable<OTAWebhookLog> query = _unitOfWork.OTAWebhookLogs
                    .GetAll(w => w.IsDeadLetter && !w.IsDeleted)
                    .Include(w => w.OTAIntegration)
                    .OrderByDescending(w => w.DeadLetterAt);

                if (!string.IsNullOrEmpty(providerCode))
                {
                    query = query.Where(w => w.ProviderCode == providerCode);
                }

                var totalCount = await query.CountAsync();
                var webhooks = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(w => new
                    {
                        w.Id,
                        w.ProviderCode,
                        w.IdempotencyKey,
                        w.EventType,
                        w.ReservationId,
                        w.Status,
                        w.RetryCount,
                        w.MaxRetries,
                        w.ErrorMessage,
                        w.DeadLetterAt,
                        w.ProcessedAt,
                        IntegrationName = w.OTAIntegration != null ? w.OTAIntegration.ProviderName : null
                    })
                    .ToListAsync();

                return ApiResponse<object>.SuccessResponse(new
                {
                    Data = webhooks,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dead-letter webhooks");
                return ApiResponse<object>.Fail($"Failed to get dead-letter webhooks: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RetryDeadLetterWebhookAsync(int webhookLogId)
        {
            try
            {
                var webhookLog = await _unitOfWork.OTAWebhookLogs
                    .GetAll(w => w.Id == webhookLogId && w.IsDeadLetter && !w.IsDeleted)
                    .Include(w => w.OTAIntegration)
                    .FirstOrDefaultAsync();

                if (webhookLog == null)
                    return ApiResponse<bool>.Fail("Webhook log not found or not in dead-letter queue");

                if (webhookLog.OTAIntegration == null || !webhookLog.OTAIntegration.IsActive)
                    return ApiResponse<bool>.Fail("OTA integration not found or inactive");

                // Reset webhook log for retry
                webhookLog.IsDeadLetter = false;
                webhookLog.DeadLetterAt = null;
                webhookLog.Status = "Pending";
                webhookLog.RetryCount = 0;
                webhookLog.NextRetryAt = null;
                webhookLog.ProcessedAt = DateTime.UtcNow;
                _unitOfWork.OTAWebhookLogs.Update(webhookLog);
                await _unitOfWork.CommitAsync();

                // Process webhook again
                var result = await ProcessWebhookAsync(
                    webhookLog.ProviderCode,
                    webhookLog.Payload,
                    webhookLog.Signature,
                    webhookLog.IdempotencyKey,
                    webhookLog.IpAddress,
                    webhookLog.UserAgent);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retry dead-letter webhook {WebhookLogId}", webhookLogId);
                return ApiResponse<bool>.Fail($"Failed to retry dead-letter webhook: {ex.Message}");
            }
        }

        // Placeholder methods for rate limiting and circuit breaker (to be implemented)
        private bool CheckRateLimit(string providerCode)
        {
            // TODO: Implement rate limiting logic
            return true;
        }

        private bool CheckCircuitBreaker(string providerCode)
        {
            // TODO: Implement circuit breaker logic
            return true;
        }

        private void RecordCircuitBreakerSuccess(string providerCode)
        {
            // TODO: Record successful circuit breaker operation
        }

        private void RecordCircuitBreakerFailure(string providerCode)
        {
            // TODO: Record failed circuit breaker operation
        }
    }
}