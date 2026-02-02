using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.OTA.BookingDotCom.Dtos;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.OTA.BookingDotCom
{


    public class BookingDotComService : IBookingDotComService
    {
        private readonly ILogger<BookingDotComService> _logger;
        // In a real scenario, we might inject OTAIntegrationService or IUnitOfWork here
        // to look up secrets from the DB if they aren't passed in directly.
        // For processing webhooks, we usually get the secret from the integration config.

        public BookingDotComService(ILogger<BookingDotComService> logger)
        {
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> HandleWebhookAsync(string payload, string signature)
        {
            // Logic to orchestrate: Validate -> Parse -> Process
            // This is largely handled by OTAWebhookHandler generically in the current architecture.
            // But this service can provide specific parsing logic.
            
            try 
            {
                var bookingPayload = ParsePayload(payload);
                 if (bookingPayload == null)
                    return ApiResponse<bool>.Fail("Failed to parse Booking.com payload");
                
                // Further business logic could go here, e.g., specifc field mapping validation
                
                return ApiResponse<bool>.SuccessResponse(true, "Payload parsed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Booking.com webhook");
                return ApiResponse<bool>.Fail(ex.Message);
            }
        }

        public bool ValidateSignature(string payload, string signature, string secret)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature))
                return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return computedSignature == signature.ToLowerInvariant();
        }

        public BookingWebhookPayloadDto ParsePayload(string payload)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<BookingWebhookPayloadDto>(payload, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for Booking.com payload");
                throw;
            }
        }
    }
}
