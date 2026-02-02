using FluentAssertions;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq.Expressions;

namespace GuestFlow.Application.Tests.Operations.PMS
{
    public class PMSPollingBackgroundServiceTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPMSSyncService> _pmsSyncServiceMock;
        private readonly Mock<ILogger<PMSPollingBackgroundService>> _loggerMock;

        public PMSPollingBackgroundServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _pmsSyncServiceMock = new Mock<IPMSSyncService>();
            _loggerMock = new Mock<ILogger<PMSPollingBackgroundService>>();

            // Setup Service Scope
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
            _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

            // Setup DI Resolution
            _serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork))).Returns(_unitOfWorkMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IPMSSyncService))).Returns(_pmsSyncServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Trigger_Sync_For_Active_Polling_Integrations()
        {
            // Arrange
            var activeIntegration = new PMSIntegration
            {
                Id = 1,
                ProviderName = "Opera",
                IsActive = true,
                SyncMode = PMSSyncMode.Polling,
                PollingIntervalMinutes = 1
            };

            var integrations = new List<PMSIntegration> { activeIntegration }.AsQueryable();

            // Mock Repositories
            _unitOfWorkMock.Setup(x => x.PMSIntegrations.GetAll(It.IsAny<Expression<Func<PMSIntegration, bool>>>()))
                .Returns(integrations);

            _pmsSyncServiceMock.Setup(x => x.SyncReservationsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new Models.Responses.ApiResponse<bool> { Success = true });
            
            _pmsSyncServiceMock.Setup(x => x.SyncGuestsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new Models.Responses.ApiResponse<bool> { Success = true });

            _pmsSyncServiceMock.Setup(x => x.SyncRoomsStatusAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new Models.Responses.ApiResponse<bool> { Success = true });

            var service = new PMSPollingBackgroundService(_serviceProviderMock.Object, _loggerMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            // Run for a short time to allow one polling cycle
            var task = service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(100); 
            cancellationTokenSource.Cancel();
            await task;

            // Assert
            _pmsSyncServiceMock.Verify(x => x.SyncReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce, "Should sync reservations");
            _pmsSyncServiceMock.Verify(x => x.SyncGuestsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce, "Should sync guests");
        }
    }
}
