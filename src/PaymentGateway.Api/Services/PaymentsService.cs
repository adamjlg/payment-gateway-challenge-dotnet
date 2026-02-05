using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Validators;


namespace PaymentGateway.Api.Services;

// business logic for repository
public class PaymentsService
{
    private readonly PaymentsRepository _repository;
    private readonly PostPaymentRequestValidator _validator;
    private readonly IBankClient _bankClient;

    public PaymentsService(
       PaymentsRepository repository,
       PostPaymentRequestValidator validator,
       IBankClient bankClient)
    {
        _repository = repository;
        _validator = validator;
        _bankClient = bankClient;
    }

    public async Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request)
    {
        // validate inputs
        if (!_validator.TryValidate(request, out var errors))
        {
            var rejected = CreateInternalPayment(request, PaymentStatus.Rejected);
            _repository.Add(rejected);
            return MapToPostPaymentResponse(rejected, errors);
        }

        // call bank via abstraction
        PaymentStatus status;
        try
        {
            status = await _bankClient.ProcessAsync(request);
        }
        catch
        {
            status = PaymentStatus.Rejected;
        }

            // store payment in memory
        var payment = CreateInternalPayment(request, status);
        _repository.Add(payment);

        return MapToPostPaymentResponse(payment);


    }

    private InternalPayment CreateInternalPayment(PostPaymentRequest request, PaymentStatus status)
    {
        var month = request.ExpiryMonth;
        var year = request.ExpiryYear;

        return new InternalPayment
        {
            Id = Guid.NewGuid(),
            CardNumber = request.CardNumber ?? string.Empty,
            ExpiryMonth = month,
            ExpiryYear = year,
            Currency = request.Currency ?? string.Empty,
            Amount = request.Amount,
            Cvv = request.Cvv ?? string.Empty,
            Status = status
        };
    }

    private PostPaymentResponse MapToPostPaymentResponse(InternalPayment payment, string[]? validationErrors = null)
    {
        return new PostPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = GetLastFour(payment.CardNumber),
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount,
            ValidationErrors = validationErrors
        };
    }

    private static string GetLastFour(string? cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber)) return string.Empty;
        return cardNumber.Length <= 4 ? cardNumber : cardNumber[^4..];
    }


    public GetPaymentResponse GetPayment(Guid id)
    {
        var payment = _repository.Get(id);
        if (payment == null) return null;

        return new GetPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = GetLastFour(payment.CardNumber),
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };

    }
}
