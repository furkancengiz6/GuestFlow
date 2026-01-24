using FluentAssertions;
using GuestFlow.Application.Operations.Reports;
using GuestFlow.Application.Tests.Helpers;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Reports;

public class ReportsServiceTests : TestBase
{
    private readonly Mock<IRepository<DailyRevenueEntity>> _dailyRevenueRepositoryMock;
    private readonly Mock<IRepository<PaymentEntity>> _paymentRepositoryMock;
    private readonly Mock<IRepository<GuestEntity>> _guestRepositoryMock;
    private readonly Mock<IRepository<CityTourEntity>> _cityTourRepositoryMock;
    private readonly Mock<IRepository<YachtTourEntity>> _yachtTourRepositoryMock;
    private readonly Mock<IRepository<TransferEntity>> _transferRepositoryMock;
    private readonly Mock<IRepository<InvoicesEntity>> _invoiceRepositoryMock;
    private readonly Mock<IRepository<InvoiceItemEntity>> _invoiceItemRepositoryMock;
    private readonly Mock<IRepository<JournalEntry>> _journalEntryRepositoryMock;
    private readonly Mock<IRepository<JournalLine>> _journalLineRepositoryMock;
    private readonly Mock<IRepository<CityEntity>> _cityRepositoryMock;
    private readonly Mock<IRepository<PersonnelEntity>> _personnelRepositoryMock;
    private readonly Mock<ILogger<ReportsService>> _loggerMock;
    private readonly ReportsService _reportsService;

    public ReportsServiceTests()
    {
        _dailyRevenueRepositoryMock = CreateMock<IRepository<DailyRevenueEntity>>();
        _paymentRepositoryMock = CreateMock<IRepository<PaymentEntity>>();
        _guestRepositoryMock = CreateMock<IRepository<GuestEntity>>();
        _cityTourRepositoryMock = CreateMock<IRepository<CityTourEntity>>();
        _yachtTourRepositoryMock = CreateMock<IRepository<YachtTourEntity>>();
        _transferRepositoryMock = CreateMock<IRepository<TransferEntity>>();
        _invoiceRepositoryMock = CreateMock<IRepository<InvoicesEntity>>();
        _invoiceItemRepositoryMock = CreateMock<IRepository<InvoiceItemEntity>>();
        _journalEntryRepositoryMock = CreateMock<IRepository<JournalEntry>>();
        _journalLineRepositoryMock = CreateMock<IRepository<JournalLine>>();
        _cityRepositoryMock = CreateMock<IRepository<CityEntity>>();
        _personnelRepositoryMock = CreateMock<IRepository<PersonnelEntity>>();
        _loggerMock = CreateMock<ILogger<ReportsService>>();

        _reportsService = new ReportsService(
            _dailyRevenueRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _guestRepositoryMock.Object,
            _cityTourRepositoryMock.Object,
            _yachtTourRepositoryMock.Object,
            _transferRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _invoiceItemRepositoryMock.Object,
            _journalEntryRepositoryMock.Object,
            _journalLineRepositoryMock.Object,
            _cityRepositoryMock.Object,
            _personnelRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetRevenueSummaryAsync_ShouldReturnMultiCurrencyRevenue_WhenPaymentsExist()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var payments = new List<PaymentEntity>
        {
            new PaymentEntity { Id = 1, Amount = 1000m, Currency = "USD", Status = PaymentStatus.Completed, PaymentDate = DateTime.UtcNow, TransferId = 1 },
            new PaymentEntity { Id = 2, Amount = 2000m, Currency = "EUR", Status = PaymentStatus.Completed, PaymentDate = DateTime.UtcNow, CityTourId = 1 },
            new PaymentEntity { Id = 3, Amount = 1500m, Currency = "USD", Status = PaymentStatus.Completed, PaymentDate = DateTime.UtcNow, YachtTourId = 1 },
            new PaymentEntity { Id = 4, Amount = 500m, Currency = "TRY", Status = PaymentStatus.Completed, PaymentDate = DateTime.UtcNow }
        };

        SetupPaymentQuery(payments, startDate, endDate);
        SetupServiceCountQueries(1, 1, 1);

        // Act
        var result = await _reportsService.GetRevenueSummaryAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenueByCurrency.Should().ContainKey("USD").WhoseValue.Should().Be(2500m); // 1000 + 1500
        result.TotalRevenueByCurrency.Should().ContainKey("EUR").WhoseValue.Should().Be(2000m);
        result.TotalRevenueByCurrency.Should().ContainKey("TRY").WhoseValue.Should().Be(500m);
        result.TransferRevenueByCurrency.Should().ContainKey("USD").WhoseValue.Should().Be(1000m);
        result.CityTourRevenueByCurrency.Should().ContainKey("EUR").WhoseValue.Should().Be(2000m);
        result.YachtTourRevenueByCurrency.Should().ContainKey("USD").WhoseValue.Should().Be(1500m);
    }

    [Fact]
    public async Task GetRevenueSummaryAsync_ShouldHandleEmptyData()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        SetupPaymentQuery(new List<PaymentEntity>(), startDate, endDate);
        SetupServiceCountQueries(0, 0, 0);

        // Act
        var result = await _reportsService.GetRevenueSummaryAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenueByCurrency.Should().BeEmpty();
        result.TotalPaymentCount.Should().Be(0);
        result.TotalBookings.Should().Be(0);
    }

    [Fact]
    public async Task GetVatAccrualReportAsync_ShouldCalculateVAT_WhenInvoiceItemsExist()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var invoices = new List<InvoicesEntity>
        {
            new InvoicesEntity { Id = 1, IssueDate = DateTime.UtcNow, Currency = "USD", InvoiceItems = new List<InvoiceItemEntity> { new InvoiceItemEntity { Amount = 1000m, VatRate = 0.20m, VatAmount = 200m } } },
            new InvoicesEntity { Id = 2, IssueDate = DateTime.UtcNow, Currency = "EUR", InvoiceItems = new List<InvoiceItemEntity> { new InvoiceItemEntity { Amount = 2000m, VatRate = 0.18m, VatAmount = 360m } } }
        };

        SetupInvoiceQuery(invoices, startDate, endDate);
        SetupJournalEntryQuery(new List<JournalEntry>());

        // Act
        var result = await _reportsService.GetVatAccrualReportAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalVatByCurrency.Should().ContainKey("USD").WhoseValue.Should().Be(200m);
        result.TotalVatByCurrency.Should().ContainKey("EUR").WhoseValue.Should().Be(360m);
    }

    [Fact]
    public async Task GetVatPeriodReportAsync_ShouldGroupByPeriod_WhenDataExists()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-3);
        var endDate = DateTime.UtcNow;

        var invoices = new List<InvoicesEntity>
        {
            new InvoicesEntity { Id = 1, IssueDate = DateTime.UtcNow.AddMonths(-2), Currency = "USD", InvoiceItems = new List<InvoiceItemEntity> { new InvoiceItemEntity { Amount = 1000m, VatAmount = 200m } } },
            new InvoicesEntity { Id = 2, IssueDate = DateTime.UtcNow.AddMonths(-1), Currency = "EUR", InvoiceItems = new List<InvoiceItemEntity> { new InvoiceItemEntity { Amount = 2000m, VatAmount = 360m } } }
        };

        SetupInvoiceQuery(invoices, startDate, endDate);

        // Act
        var result = await _reportsService.GetVatPeriodReportAsync(startDate, endDate, "Monthly");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    private void SetupPaymentQuery(List<PaymentEntity> payments, DateTime startDate, DateTime endDate)
    {
        var filtered = payments.Where(p => 
            p.PaymentDate.Date >= startDate.Date && 
            p.PaymentDate.Date <= endDate.Date && 
            p.Status == PaymentStatus.Completed && 
            !p.IsDeleted).ToList();

        _paymentRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(filtered.BuildMockQueryable().Object);
    }

    private void SetupServiceCountQueries(int transferCount, int cityTourCount, int yachtTourCount)
    {
        var transfers = Enumerable.Range(0, transferCount).Select(i => new TransferEntity { Id = i + 1, TransferDate = DateTime.UtcNow }).ToList();
        var cityTours = Enumerable.Range(0, cityTourCount).Select(i => new CityTourEntity { Id = i + 1, TourDate = DateTime.UtcNow }).ToList();
        var yachtTours = Enumerable.Range(0, yachtTourCount).Select(i => new YachtTourEntity { Id = i + 1, TourDate = DateTime.UtcNow }).ToList();

        _transferRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(transfers.BuildMockQueryable().Object);

        _cityTourRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(cityTours.BuildMockQueryable().Object);

        _yachtTourRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(yachtTours.BuildMockQueryable().Object);
    }

    private void SetupInvoiceItemQuery(List<InvoiceItemEntity> items, DateTime startDate, DateTime endDate)
    {
        var filtered = items.Where(i => 
            i.CreatedDate >= startDate && 
            i.CreatedDate <= endDate && 
            !i.IsDeleted).ToList();

        _invoiceItemRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(filtered.BuildMockQueryable().Object);
    }

    private void SetupInvoiceQuery(List<InvoicesEntity> invoices, DateTime startDate, DateTime endDate)
    {
        var filtered = invoices.Where(i => 
            i.IssueDate >= startDate && 
            i.IssueDate <= endDate.AddDays(1).AddTicks(-1) && 
            !i.IsDeleted).ToList();

        _invoiceRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(filtered.BuildMockQueryable().Object);
    }

    private void SetupJournalEntryQuery(List<JournalEntry> entries)
    {
        _journalEntryRepositoryMock.Setup(r => r.GetAll(It.IsAny<Expression<Func<JournalEntry, bool>>>(), It.IsAny<bool>()))
            .Returns<Expression<Func<JournalEntry, bool>>, bool>((predicate, includeDeleted) => 
            {
                var query = entries.AsQueryable();
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                return query.BuildMockQueryable().Object;
            });
    }
}
