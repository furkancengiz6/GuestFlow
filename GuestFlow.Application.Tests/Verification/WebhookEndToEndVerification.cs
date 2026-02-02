using System;
using System.Text.Json;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.OTA;
using GuestFlow.Application.Operations.OTA.BookingDotCom.Dtos;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using System.Net.Http;

namespace GuestFlow.Application.Tests.Verification
{
    public class WebhookEndToEndTests
    {
        [Fact]
        public async Task Verify_Webhook_Flow_EndToEnd()
        {
             // 1. Setup In-Memory Database
            var options = new DbContextOptionsBuilder<GuestFlowDbContext>()
                .UseInMemoryDatabase(databaseName: "GuestFlow_Verification_" + Guid.NewGuid())
                .Options;

            using var context = new GuestFlowDbContext(options);
            var unitOfWork = new UnitOfWork(context);

            // 2. Setup Dependencies
            var loggerFactory = new NullLoggerFactory();
            var logger = new NullLogger<OTAChannelManagerService>();
            var integrationLogger = new NullLogger<OTAIntegrationService>();
            
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var pmsServiceMock = new Mock<IPMSIntegrationService>();

            // 3. Setup OTA Integration in DB
            var integration = new OTAIntegration
            {
                ProviderName = "Booking.com",
                ProviderCode = "BKG",
                ApiSecret = "test_secret",
                IsActive = true,
                WebhookUrl = "http://localhost/webhook"
            };
            context.OTAIntegrations.Add(integration);
            context.SaveChanges();

            // 4. Instantiate Services
            var mappingService = new OTAReservationMappingService(unitOfWork, new NullLogger<OTAReservationMappingService>());
            
            var channelManager = new OTAChannelManagerService(
                unitOfWork,
                pmsServiceMock.Object,
                mappingService,
                httpClientFactoryMock.Object,
                logger
            );

            var otaIntegrationService = new OTAIntegrationService(
                unitOfWork,
                httpClientFactoryMock.Object,
                pmsServiceMock.Object,
                channelManager,
                integrationLogger,
                loggerFactory
            );

            // 5. Prepare Webhook Payload
            var reservationId = "RES-12345";
            var payloadObj = new
            {
                event_type = "RESERVATION_CREATED",
                reservation_id = reservationId,
                guest_name = "Test Guest",
                total_price = 250.50,
                hotel_id = "1001"
            };
            var payloadJson = JsonSerializer.Serialize(payloadObj);

            // 6. Calculate Signature
            var secret = "test_secret";
            string signature;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payloadJson));
                signature = Convert.ToHexString(hash).ToLowerInvariant();
            }

            // 7. Execute Webhook Processing
            var result = await otaIntegrationService.ProcessWebhookAsync("BKG", payloadJson, signature);

            // 8. Verify Results
            Assert.True(result.Success, $"Webhook processing failed: {result.Message}");

            // 9. Verify Persistence
            var savedReservation = await context.OTAReservations.FirstOrDefaultAsync(r => r.OTAReservationId == reservationId);
            Assert.NotNull(savedReservation);
            Assert.Equal("Test Guest", savedReservation.GuestName);
        }
    }
}
