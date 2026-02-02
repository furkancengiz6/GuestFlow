using GuestFlow.Application.Operations.OTA.BookingDotCom;
using GuestFlow.Application.Operations.OTA.Expedia;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.OTA
{
    public interface IOTAAdapterFactory
    {
        BaseOTAAdapter CreateAdapter(OTAIntegration integration);
    }

    public class OTAAdapterFactory : IOTAAdapterFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IBookingDotComService _bookingService;
        private readonly IExpediaService _expediaService;

        public OTAAdapterFactory(
            IHttpClientFactory httpClientFactory, 
            ILoggerFactory loggerFactory,
            IBookingDotComService bookingService,
            IExpediaService expediaService)
        {
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
            _bookingService = bookingService;
            _expediaService = expediaService;
        }

        public BaseOTAAdapter CreateAdapter(OTAIntegration integration)
        {
            var providerCode = integration.ProviderCode.ToUpperInvariant();
            
            return providerCode switch
            {
                "BKG" or "BOOKING" or "BOOKINGCOM" => new BookingComAdapter(integration, 
                    _httpClientFactory,
                    _loggerFactory.CreateLogger<BookingComAdapter>(),
                    _bookingService),
                "EXP" or "EXPEDIA" => new ExpediaAdapter(integration,
                    _httpClientFactory,
                    _loggerFactory.CreateLogger<ExpediaAdapter>(),
                    _expediaService),
                _ => throw new NotSupportedException($"OTA provider '{integration.ProviderCode}' is not supported")
            };
        }
    }
}
