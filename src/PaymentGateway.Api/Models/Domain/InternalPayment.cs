namespace PaymentGateway.Api.Models.Domain;

public class InternalPayment
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }   // store full internally
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; } // in minor units
    public string Cvv { get; set; } // store internally
    public PaymentStatus Status { get; set; }       // authorized/declined/rejected
}
