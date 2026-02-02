using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Application.Operations.OTA;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.OTA
{
    public class OTAChannelManagerServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPMSIntegrationService> _mockPmsIntegrationService;
        private readonly Mock<IOTAReservationMappingService> _mockMappingService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IOTAAdapterFactory> _mockAdapterFactory;
        private readonly Mock<ILogger<OTAChannelManagerService>> _mockLogger;
        private readonly Mock<IDynamicPricingService> _mockDynamicPricingService;
        
        private readonly Mock<IGenericRepository<PMSIntegration>> _mockPmsIntegrationRepo;
        private readonly Mock<IGenericRepository<OTAIntegration>> _mockOtaIntegrationRepo;
        private readonly Mock<IGenericRepository<OTAReservation>> _mockOtaReservationRepo;

        private readonly OTAChannelManagerService _service;

        public OTAChannelManagerServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPmsIntegrationService = new Mock<IPMSIntegrationService>();
            _mockMappingService = new Mock<IOTAReservationMappingService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockAdapterFactory = new Mock<IOTAAdapterFactory>();
            _mockLogger = new Mock<ILogger<OTAChannelManagerService>>();
            _mockDynamicPricingService = new Mock<IDynamicPricingService>();

            _mockPmsIntegrationRepo = new Mock<IGenericRepository<PMSIntegration>>();
            _mockOtaIntegrationRepo = new Mock<IGenericRepository<OTAIntegration>>();
            _mockOtaReservationRepo = new Mock<IGenericRepository<OTAReservation>>();

            _mockUnitOfWork.Setup(u => u.PMSIntegrations).Returns(_mockPmsIntegrationRepo.Object);
            _mockUnitOfWork.Setup(u => u.OTAIntegrations).Returns(_mockOtaIntegrationRepo.Object);
            _mockUnitOfWork.Setup(u => u.OTAReservations).Returns(_mockOtaReservationRepo.Object);

            _service = new OTAChannelManagerService(
                _mockUnitOfWork.Object,
                _mockPmsIntegrationService.Object,
                _mockMappingService.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object,
                _mockAdapterFactory.Object,
                _mockDynamicPricingService.Object
            );
        }

        [Fact]
        public async Task SyncAvailabilityFromPMSToOTAsAsync_ShouldSync_WhenIntegrationsExist()
        {
            // Arrange
            int pmsId = 1;
            var pmsIntegration = new PMSIntegration { Id = pmsId, IsActive = true, ProviderCode = "OPERA" };
            var otaIntegration = new OTAIntegration { Id = 10, IsActive = true, ProviderCode = "BOOKING" };

            var rooms = new List<PMSRoomStatus>
            {
                new PMSRoomStatus { RoomNumber = "101", Status = "Available", RoomType = "STD" }
            };

            _mockPmsIntegrationRepo.Setup(r => r.GetByIdAsync(pmsId)).ReturnsAsync(pmsIntegration);
            _mockPmsIntegrationService.Setup(s => s.GetRoomsStatusAsync(pmsId, It.IsAny<DateTime?>()))
                .ReturnsAsync(ApiResponse<List<PMSRoomStatus>>.SuccessResponse(rooms));

            var otaList = new List<OTAIntegration> { otaIntegration };
            var mockSet = new TestAsyncEnumerable<OTAIntegration>(otaList);
            
            _mockOtaIntegrationRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OTAIntegration, bool>>>()))
                .Returns(mockSet);

            var mockAdapter = new Mock<BaseOTAAdapter>(otaIntegration, _mockHttpClientFactory.Object, new Mock<ILogger>().Object);
            _mockAdapterFactory.Setup(f => f.CreateAdapter(It.IsAny<OTAIntegration>()))
                .Returns(mockAdapter.Object);

            mockAdapter.Setup(a => a.UpdateAvailabilityAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SyncAvailabilityFromPMSToOTAsAsync(pmsId, null);

            // Assert
            Assert.True(result.Success);
            _mockAdapterFactory.Verify(f => f.CreateAdapter(It.IsAny<OTAIntegration>()), Times.Once);
            mockAdapter.Verify(a => a.UpdateAvailabilityAsync("101", It.IsAny<DateTime>(), true), Times.Once);
        }

        [Fact]
        public async Task SyncRatesToOTAAsync_ShouldUseDynamicPricing()
        {
            // Arrange
            int otaId = 1;
            int pmsId = 2;
            var startDate = DateTime.UtcNow.Date;
            var endDate = DateTime.UtcNow.Date.AddDays(1);

            var otaIntegration = new OTAIntegration { Id = otaId, IsActive = true, ProviderCode = "AGODA" };
            var pmsIntegration = new PMSIntegration { Id = pmsId, IsActive = true, ProviderCode = "OPERA" };

            _mockOtaIntegrationRepo.Setup(r => r.GetByIdAsync(otaId)).ReturnsAsync(otaIntegration);
            _mockPmsIntegrationRepo.Setup(r => r.GetByIdAsync(pmsId)).ReturnsAsync(pmsIntegration);

            var roomTypes = new List<PMSRoomType>
            {
                new PMSRoomType { RoomTypeId = "1", Name = "Standard", BasePrice = 100m, Currency = "USD" }
            };
            
            _mockPmsIntegrationService.Setup(s => s.GetRoomTypesAsync(pmsId))
                .ReturnsAsync(ApiResponse<List<PMSRoomType>>.SuccessResponse(roomTypes));

            var mockAdapter = new Mock<BaseOTAAdapter>(otaIntegration, _mockHttpClientFactory.Object, new Mock<ILogger>().Object);
            _mockAdapterFactory.Setup(f => f.CreateAdapter(otaIntegration)).Returns(mockAdapter.Object);

            // Dynamic Pricing Mock
            _mockDynamicPricingService.Setup(d => d.CalculateRateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>()))
                .ReturnsAsync(new DynamicPricingResult { FinalRate = 120m, BaseRate = 100m, IsStopSell = false });

            // Act
            var result = await _service.SyncRatesToOTAAsync(otaId, pmsId, startDate, endDate);

            // Assert
            Assert.True(result.Success);
            _mockDynamicPricingService.Verify(d => d.CalculateRateAsync(1, It.IsAny<DateTime>(), 100m), Times.Exactly(2)); // 2 days
            mockAdapter.Verify(a => a.UpdateRatesAsync("1", It.IsAny<DateTime>(), 120m, "USD"), Times.Exactly(2));
        }

        [Fact]
        public async Task BroadcastStopSellAsync_ShouldCallAdapterForActivatedChannels()
        {
            // Arrange
            int hotelId = 1;
            var startDate = DateTime.UtcNow.Date;
            var endDate = DateTime.UtcNow.Date.AddDays(1);
            var otaIntegration = new OTAIntegration { Id = 1, IsActive = true, ProviderName = "Booking" };
            var pmsIntegration = new PMSIntegration { Id = 1, IsActive = true, HotelId = hotelId.ToString() };

            var otaList = new List<OTAIntegration> { otaIntegration };
            _mockOtaIntegrationRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OTAIntegration, bool>>>()))
                .Returns(new TestAsyncEnumerable<OTAIntegration>(otaList));

            _mockPmsIntegrationRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<PMSIntegration, bool>>>()))
                .Returns(new TestAsyncEnumerable<PMSIntegration>(new List<PMSIntegration> { pmsIntegration }));

            _mockPmsIntegrationService.Setup(s => s.GetRoomsStatusAsync(pmsIntegration.Id, null))
                .ReturnsAsync(ApiResponse<List<PMSRoomStatus>>.SuccessResponse(new List<PMSRoomStatus>
                {
                    new PMSRoomStatus { RoomType = "DELUXE" },
                    new PMSRoomStatus { RoomType = "STD" }
                }));

            var mockAdapter = new Mock<BaseOTAAdapter>(otaIntegration, _mockHttpClientFactory.Object, new Mock<ILogger>().Object);
            _mockAdapterFactory.Setup(f => f.CreateAdapter(It.IsAny<OTAIntegration>()))
                .Returns(mockAdapter.Object);

            // Act
            var result = await _service.BroadcastStopSellAsync(hotelId, startDate, endDate);

            // Assert
            Assert.True(result.Success);
            // Should be called for 2 dates * 2 room types = 4 times
            mockAdapter.Verify(a => a.UpdateAvailabilityAsync(It.IsAny<string>(), It.IsAny<DateTime>(), false), Times.Exactly(4));
        }
    }
    
    internal class TestAsyncQueryProvider<TEntity> : Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, System.Threading.CancellationToken cancellationToken = default)
        {
            // ExecuteAsync logic
             var resultType = typeof(TResult).GetGenericArguments()[0];
             var executionResult = typeof(IQueryProvider)
                 .GetMethod(
                     name: nameof(IQueryProvider.Execute),
                     genericParameterCount: 1,
                     types: new[] { typeof(Expression) })
                 .MakeGenericMethod(resultType)
                 .Invoke(this, new[] { expression });

             return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                 .MakeGenericMethod(resultType)
                 .Invoke(null, new[] { executionResult });
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestAsyncQueryProvider<T>(this); }
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }

        public T Current
        {
            get { return _inner.Current; }
        }
    }
}
