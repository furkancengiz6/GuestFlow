using FluentAssertions;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Types;
using System;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Validation;

/// <summary>
/// Unit tests for InputValidationService - Security hardening from Phase 1
/// </summary>
public class InputValidationServiceTests
{
    private readonly InputValidationService _validationService;

    public InputValidationServiceTests()
    {
        _validationService = new InputValidationService();
    }

    #region Secure Input Validation Tests

    [Theory]
    [InlineData("SELECT * FROM Users", false)]
    [InlineData("INSERT INTO Users VALUES", false)]
    [InlineData("UPDATE Users SET", false)]
    [InlineData("DELETE FROM Users", false)]
    [InlineData("DROP TABLE Users", false)]
    [InlineData("<script>alert('xss')</script>", false)]
    [InlineData("javascript:alert('xss')", false)]
    [InlineData("Normal text input", true)]
    [InlineData("Valid name with spaces", true)]
    [InlineData("Valid-name-with-dashes", true)]
    [InlineData("", true)] // Empty string is allowed
    [InlineData(null, true)] // Null is allowed (handled in validation)
    public void ValidateSecureInput_ShouldDetectInjectionAttacks(string input, bool expectedResult)
    {
        // Act
        var result = _validationService.ValidateSecureInput(input, "TestField");

        // Assert
        result.IsSuccess.Should().Be(expectedResult);
    }

    [Fact]
    public void ValidateSecureInput_ShouldRejectOverlyLongInput()
    {
        // Arrange
        var longInput = new string('A', 1001); // Over 1000 limit

        // Act
        var result = _validationService.ValidateSecureInput(longInput, "TestField");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cannot exceed 1000 characters");
    }

    [Fact]
    public void ValidateSecureInput_ShouldAcceptValidLengthInput()
    {
        // Arrange
        var validInput = new string('A', 500); // Under limit

        // Act
        var result = _validationService.ValidateSecureInput(validInput, "TestField");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateSecureInput_ShouldRejectNullBytes()
    {
        // Arrange
        var inputWithNullByte = "Normal text" + '\0' + "with null byte";

        // Act
        var result = _validationService.ValidateSecureInput(inputWithNullByte, "TestField");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("invalid characters");
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("user+tag@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@example.com", false)]
    [InlineData("user@", false)]
    [InlineData("user@.com", false)]
    [InlineData("user@example..com", false)]
    public void ValidateEmail_ShouldValidateEmailFormat(string email, bool expectedResult)
    {
        // Act
        var result = _validationService.ValidateEmail(email);

        // Assert
        result.IsSuccess.Should().Be(expectedResult);
        if (!expectedResult)
        {
            result.Message.Should().Contain("Invalid email format");
        }
    }

    [Fact]
    public void ValidateEmail_ShouldRejectNullOrEmptyEmail()
    {
        // Act & Assert
        var nullResult = _validationService.ValidateEmail(null);
        var emptyResult = _validationService.ValidateEmail("");

        nullResult.IsSuccess.Should().BeFalse();
        emptyResult.IsSuccess.Should().BeFalse();
        nullResult.Message.Should().Contain("required");
        emptyResult.Message.Should().Contain("required");
    }

    [Fact]
    public void ValidateEmail_ShouldApplySecurityValidation()
    {
        // Arrange
        var maliciousEmail = "user@example.com<script>alert('xss')</script>";

        // Act
        var result = _validationService.ValidateEmail(maliciousEmail);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("security");
    }

    #endregion

    #region Phone Number Validation Tests

    [Theory]
    [InlineData("+905551234567", true)]
    [InlineData("05551234567", true)]
    [InlineData("5551234567", true)]
    [InlineData("+15551234567", true)]
    [InlineData("0555 123 45 67", true)] // With spaces
    [InlineData("(555) 123-4567", true)]
    [InlineData("invalid-phone", false)]
    [InlineData("abc123", false)]
    [InlineData("", true)] // Optional field
    [InlineData(null, true)] // Optional field
    public void ValidatePhoneNumber_ShouldValidatePhoneFormat(string phone, bool expectedResult)
    {
        // Act
        var result = _validationService.ValidatePhoneNumber(phone);

        // Assert
        result.IsSuccess.Should().Be(expectedResult);
        if (!expectedResult && !string.IsNullOrEmpty(phone))
        {
            result.Message.Should().Contain("Invalid phone number format");
        }
    }

    #endregion

    #region Name Validation Tests

    [Theory]
    [InlineData("John Doe", true)]
    [InlineData("José María", true)]
    [InlineData("O'Connor", true)]
    [InlineData("Anna-Marie", true)]
    [InlineData("Test Name", true)]
    [InlineData("Name123", false)] // Numbers not allowed
    [InlineData("Name@Symbol", false)] // Special chars not allowed
    [InlineData("SELECT", false)] // SQL keywords not allowed
    public void ValidateName_ShouldValidateNameFormat(string name, bool expectedResult)
    {
        // Act
        var result = _validationService.ValidateName(name, "Full Name");

        // Assert
        result.IsSuccess.Should().Be(expectedResult);
        if (!expectedResult)
        {
            result.Message.Should().Contain("can only contain letters");
        }
    }

    [Fact]
    public void ValidateName_ShouldRejectNullOrEmptyName()
    {
        // Act & Assert
        var nullResult = _validationService.ValidateName(null, "Full Name");
        var emptyResult = _validationService.ValidateName("", "Full Name");

        nullResult.IsSuccess.Should().BeFalse();
        emptyResult.IsSuccess.Should().BeFalse();
        nullResult.Message.Should().Contain("required");
    }

    #endregion

    #region Address Validation Tests

    [Theory]
    [InlineData("123 Main Street, City, Country", true)]
    [InlineData("Valid address with numbers 123", true)]
    [InlineData("Address with dashes-and spaces", true)]
    [InlineData("SELECT * FROM", false)] // SQL injection
    [InlineData("<script>alert('xss')</script>", false)] // XSS
    public void ValidateAddress_ShouldValidateAddressFormat(string address, bool expectedResult)
    {
        // Act
        var result = _validationService.ValidateAddress(address, "Address");

        // Assert
        result.IsSuccess.Should().Be(expectedResult);
        if (!expectedResult)
        {
            result.Message.Should().Contain("security");
        }
    }

    [Fact]
    public void ValidateAddress_ShouldRejectNullOrEmptyAddress()
    {
        // Act & Assert
        var nullResult = _validationService.ValidateAddress(null, "Address");
        var emptyResult = _validationService.ValidateAddress("", "Address");

        nullResult.IsSuccess.Should().BeFalse();
        emptyResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ValidateAddress_ShouldEnforceLengthLimit()
    {
        // Arrange
        var longAddress = new string('A', 501); // Over 500 limit

        // Act
        var result = _validationService.ValidateAddress(longAddress, "Address");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cannot exceed 500 characters");
    }

    #endregion

    #region Amount Validation Tests

    [Theory]
    [InlineData(100.50, true)]
    [InlineData(0.01, true)]
    [InlineData(999999.99, true)]
    [InlineData(-100, false)] // Negative not allowed
    [InlineData(1000000, false)] // Too high
    public void ValidateAmount_ShouldValidateAmountRange(decimal amount, bool expectedResult)
    {
        // Act
        var result = _validationService.ValidateAmount(amount, "Amount");

        // Assert
        result.IsSuccess.Should().Be(expectedResult);
        if (!expectedResult)
        {
            if (amount < 0)
                result.Message.Should().Contain("cannot be negative");
            else
                result.Message.Should().Contain("cannot exceed");
        }
    }

    #endregion

    #region Date Range Validation Tests

    [Fact]
    public void ValidateDateRange_ShouldAcceptValidDateRange()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(-10);
        var endDate = DateTime.Now.AddDays(10);

        // Act
        var result = _validationService.ValidateDateRange(startDate, endDate, "Test Period");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateDateRange_ShouldRejectInvalidDateRange()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(10);
        var endDate = DateTime.Now.AddDays(-10); // Start after end

        // Act
        var result = _validationService.ValidateDateRange(startDate, endDate, "Test Period");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cannot be after end date");
    }

    [Fact]
    public void ValidateDateRange_ShouldRejectTooOldStartDate()
    {
        // Arrange
        var startDate = DateTime.Now.AddYears(-2); // More than 1 year ago
        var endDate = DateTime.Now;

        // Act
        var result = _validationService.ValidateDateRange(startDate, endDate, "Test Period");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cannot be more than 1 year");
    }

    [Fact]
    public void ValidateDateRange_ShouldRejectTooFutureEndDate()
    {
        // Arrange
        var startDate = DateTime.Now;
        var endDate = DateTime.Now.AddYears(3); // More than 2 years ahead

        // Act
        var result = _validationService.ValidateDateRange(startDate, endDate, "Test Period");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cannot be more than 2 years");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ComprehensiveInputValidation_ShouldHandleComplexScenarios()
    {
        // Test various malicious inputs
        var maliciousInputs = new[]
        {
            "Normal input",
            "SELECT * FROM Users",
            "<script>alert('xss')</script>",
            "UNION SELECT password FROM users",
            "javascript:void(0)",
            "data:text/html,<script>alert('xss')</script>",
            "../../../etc/passwd",
            "Normal input with <b>bold</b> text"
        };

        foreach (var input in maliciousInputs)
        {
            var result = _validationService.ValidateSecureInput(input, "TestInput");
            var isMalicious = input.Contains("SELECT") ||
                             input.Contains("<script>") ||
                             input.Contains("javascript:") ||
                             input.Contains("UNION") ||
                             input.Contains("../../../");

            result.IsSuccess.Should().Be(!isMalicious);
        }
    }

    #endregion
}