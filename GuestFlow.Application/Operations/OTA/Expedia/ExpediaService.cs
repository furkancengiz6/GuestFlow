using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.OTA.Expedia
{
    public class ExpediaService : IExpediaService
    {
        private readonly ILogger<ExpediaService> _logger;

        public ExpediaService(ILogger<ExpediaService> logger)
        {
            _logger = logger;
        }

        public bool ValidateSignature(string payload, string signature, string secret)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
                return false;

            try
            {
                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToBase64String(hash);
                
                return signature == computedSignature;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error validating Expedia signature");
                 return false;
            }
        }

        public JsonElement ParsePayload(string payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload)) return new JsonElement();
                return JsonSerializer.Deserialize<JsonElement>(payload);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing Expedia payload");
                throw;
            }
        }
    }
}
