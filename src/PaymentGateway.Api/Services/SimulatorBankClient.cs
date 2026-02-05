using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

// http client that talks to the external bank simulator
// uses expected json keys
public sealed class SimulatorBankClient : IBankClient
{
    private readonly HttpClient _httpClient;

    public SimulatorBankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentStatus> ProcessAsync(PostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            card_number = request.CardNumber,
            expiry_date = request.ExpiryDate,
            currency = request.Currency,
            amount = request.Amount,
            cvv = request.Cvv
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/payments", payload, cancellationToken);
        }
        catch
        {
            return PaymentStatus.Rejected;
        }

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await ParseAuthorizedAsync(response, cancellationToken),
            HttpStatusCode.BadRequest => PaymentStatus.Rejected,
            HttpStatusCode.ServiceUnavailable => PaymentStatus.Rejected,
            _ => PaymentStatus.Rejected
        };
    }

    private static async Task<PaymentStatus> ParseAuthorizedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            if (json.TryGetProperty("authorized", out var auth))
            {
                return auth.GetBoolean() ? PaymentStatus.Authorized : PaymentStatus.Declined; // the bank returns authorized or unauthorized. we check for authorized, if false its declined. any other case is rejected
            }

            return PaymentStatus.Rejected;
        }
        catch
        {
            return PaymentStatus.Rejected;
        }
    }
}

