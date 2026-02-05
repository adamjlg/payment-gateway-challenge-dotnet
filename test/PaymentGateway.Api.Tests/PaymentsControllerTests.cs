using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

using Xunit;

namespace PaymentGateway.Api.Tests.Integration;


public class PaymentsControllerTests
{
    private class FakeBankClient : IBankClient
    {
        public bool WasCalled { get; private set; } = false;

        public Task<PaymentStatus> ProcessAsync(PostPaymentRequest request, CancellationToken cancellationToken = default)
        {
            WasCalled = true; // mark that the bank was called
            return Task.FromResult(PaymentStatus.Authorized);
        }
    }

    [Fact]
    public async Task InvalidInput() // should fail validation and not call bank
    {
        // Arrange
        var validator = new PostPaymentRequestValidator();
        var repository = new PaymentsRepository();
        var fakeBank = new FakeBankClient();

        var service = new PaymentsService(repository, validator, fakeBank);

        var request = new PostPaymentRequest
        {
            CardNumber = "123", // invalid
            ExpiryDate = "0/0", // invalid
            Currency = "XYZ",   // invalid
            Amount = 0,         // invalid
            Cvv = "12"          // invalid
        };

        // Act
        var response = await service.ProcessPaymentAsync(request); //service should fail validation and return Rejected, the bank should not be called

        // Assert
        Assert.Equal(PaymentStatus.Rejected, response.Status);
        Assert.NotEmpty(response.ValidationErrors);
        Assert.False(fakeBank.WasCalled, "Bank should not be called for invalid requests");
    }

    [Theory]
    [InlineData("1234567890123451", PaymentStatus.Authorized)] // odd
    [InlineData("1234567890123452", PaymentStatus.Declined)]   // even
    [InlineData("1234567890123450", PaymentStatus.Rejected)]   // zero
    public async Task VerifyBankStatus(string cardNumber, PaymentStatus expectedStatus) //checks status is correct based on bank response
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var now = DateTime.UtcNow;
        var request = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryDate = $"12/{now.Year + 1}", 
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var body = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        Assert.NotNull(body);
        Assert.Equal(expectedStatus, body.Status);
    }


    [Fact]
    public async Task GetId() // shoudl successfully return payment from in memory
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // post a payment first
        var now = DateTime.UtcNow;
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = $"12/{now.Year + 1}",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var post = await client.PostAsJsonAsync("/api/Payments", request);
        var postBody = await post.Content.ReadFromJsonAsync<PostPaymentResponse>();

        var get = await client.GetAsync($"/api/Payments/{postBody.Id}");
        var getBody = await get.Content.ReadFromJsonAsync<GetPaymentResponse>();

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.NotNull(getBody);
        Assert.Equal(postBody.Id, getBody.Id);
    }
}
