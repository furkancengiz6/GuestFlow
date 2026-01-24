using FluentAssertions;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Currency;

public class ExchangeRateServiceTests : TestBase
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<ExchangeRateService>> _loggerMock;
    private readonly ExchangeRateService _exchangeRateService;

    public ExchangeRateServiceTests()
    {
        _configurationMock = CreateMock<IConfiguration>();
        _loggerMock = CreateMock<ILogger<ExchangeRateService>>();

        _exchangeRateService = new ExchangeRateService(
            _configurationMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetExchangeRateAsync_ShouldReturnDefaultRate_WhenNotConfigured()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "EUR";

        // Act
        var rate = await _exchangeRateService.GetExchangeRateAsync(fromCurrency, toCurrency);

        // Assert
        rate.Should().Be(1.0m); // Default rate when not configured
    }

    [Fact]
    public async Task GetExchangeRateAsync_ShouldReturnOne_WhenSameCurrency()
    {
        // Arrange
        var currency = "USD";

        // Act
        var rate = await _exchangeRateService.GetExchangeRateAsync(currency, currency);

        // Assert
        rate.Should().Be(1.0m);
    }

    [Fact]
    public async Task ConvertAmountAsync_ShouldConvertAmount_WhenRateExists()
    {
        // Arrange
        var amount = 100m;
        var fromCurrency = "USD";
        var toCurrency = "EUR";
        var rate = 0.85m;

        _configurationMock.Setup(c => c["Accounting:ExchangeRates:USD:EUR"])
            .Returns(rate.ToString(System.Globalization.CultureInfo.InvariantCulture));

        // Act
        var converted = await _exchangeRateService.ConvertAmountAsync(amount, fromCurrency, toCurrency);

        // Assert
        converted.Should().Be(85m); // 100 * 0.85
    }

    [Fact]
    public async Task ConvertAmountAsync_ShouldReturnSameAmount_WhenSameCurrency()
    {
        // Arrange
        var amount = 100m;
        var currency = "USD";

        // Act
        var converted = await _exchangeRateService.ConvertAmountAsync(amount, currency, currency);

        // Assert
        converted.Should().Be(amount);
    }
}
