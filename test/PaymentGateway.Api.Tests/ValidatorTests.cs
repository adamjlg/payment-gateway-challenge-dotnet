using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

using Xunit;

namespace PaymentGateway.Api.Tests.Unit;

public class ValidatorTests
{
    private readonly PostPaymentRequestValidator _validator = new();

    //  different card number formats to ensure only valid numbers pass
    [Theory]
    [InlineData("", false)]                // empty card number
    [InlineData("123", false)]             // too short
    [InlineData("1234567890123456", true)] // valid
    public void CardNumber_ChecksValidity(string cardNumber, bool isValid)
    {
        var request = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryDate = "12/2030",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var result = _validator.TryValidate(request, out var errors);

        Assert.Equal(isValid, result);
        if (!isValid) Assert.Contains("Invalid card number", errors);
    }

    // ensure cards with past expiry fail and future expiry pass
    [Fact]
    public void Expiry_CheckPastAndFutureDates()
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        // past month (but valid month number): should fail with future error
        var pastMonth = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = $"{lastMonth.Month}/{lastMonth.Year}",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // past year: should fail with year error
        var pastYear = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = $"01/{now.Year - 1}",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // future date: should pass
        var future = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = $"12/{now.Year + 1}",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var pastMonthResult = _validator.TryValidate(pastMonth, out var pastMonthErrors);
        var pastYearResult = _validator.TryValidate(pastYear, out var pastYearErrors);
        var futureResult = _validator.TryValidate(future, out var futureErrors);

        Assert.False(pastMonthResult);
        Assert.Contains("Card expiry must be in the future", pastMonthErrors);

        Assert.False(pastYearResult);
        Assert.Contains("Expiry year must be current or future year", pastYearErrors);

        Assert.True(futureResult);
        Assert.Empty(futureErrors);
    }

    // only allowed currencies pass
    [Fact]
    public void Currency_CheckInvalidCurrencyFails()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2030",
            Currency = "XYZ",
            Amount = 100,
            Cvv = "123"
        };

        var result = _validator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Invalid currency", errors);
    }

    //  amount must be greater than zero
    [Fact]
    public void Amount_CheckNonPositiveFails()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2030",
            Currency = "USD",
            Amount = 0,
            Cvv = "123"
        };

        var result = _validator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Amount must be greater than 0", errors);
    }

    //  cvv length and numeric chars
    [Fact]
    public void Cvv_CheckInvalidFormatsFails()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2030",
            Currency = "USD",
            Amount = 100,
            Cvv = "12a" // non-numeric
        };

        var result = _validator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Invalid CVV", errors);
    }
}
