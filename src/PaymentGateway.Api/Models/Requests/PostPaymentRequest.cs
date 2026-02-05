using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; }

    // Expected format: "MM/YYYY"
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("cvv")]
    public string Cvv { get; set; }

    [JsonIgnore]
    public int ExpiryMonth
    {
        get
        {
            ParseExpiry(out var month, out _);
            return month;
        }
    }

    [JsonIgnore]
    public int ExpiryYear
    {
        get
        {
            ParseExpiry(out _, out var year);
            return year;
        }
    }

    private void ParseExpiry(out int month, out int year)
    {
        month = 0;
        year = 0;

        if (string.IsNullOrWhiteSpace(ExpiryDate))
        {
            return;
        }

        var parts = ExpiryDate.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return;

        if (!int.TryParse(parts[0], out month)) { month = 0; }
        if (!int.TryParse(parts[1], out year)) { year = 0; }
    }
}