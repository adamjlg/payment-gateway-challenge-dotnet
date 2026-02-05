using PaymentGateway.Api.Models.Requests;


namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator
{
    // checklist
    // card number: required, Between 14-19 chars, only numeric
    // expiry_date: required, format MM/YYYY, must be in the future
    // currency: required, 3 chars and in list above. must be size 3 list
    // amount: must be int, minor currency eg 0.01 dollars = 1. 10.50 is 1050
    // cvv: required, 3-4 chars, only numeric

    private readonly string[] _allowedCurrencies;

    public PostPaymentRequestValidator(string[] allowedCurrencies = null)
    {
        _allowedCurrencies = allowedCurrencies ?? new[] { "USD", "EUR", "GBP" }; // default currencies, assuming these are ok
    }

    public bool TryValidate(PostPaymentRequest request, out string[] errors)
    {
        var errorList = new List<string>();
        errors = errorList.ToArray();

        // lets check card number
        if (string.IsNullOrWhiteSpace(request.CardNumber) ||
           request.CardNumber.Length < 14 || request.CardNumber.Length > 19 ||
           !request.CardNumber.All(char.IsDigit))
            errorList.Add("Invalid card number");

        // expiry date
        if (string.IsNullOrWhiteSpace(request.ExpiryDate))
        {
            errorList.Add("Expiry date is required");
        }

        var month = request.ExpiryMonth;
        var year = request.ExpiryYear;

        if (month < 1 || month > 12)
            errorList.Add("Expiry month must be 1-12");

        var currentYear = DateTime.UtcNow.Year;
        if (year < currentYear)
            errorList.Add("Expiry year must be current or future year");

        // combined expiry
        if (month >= 1 && month <= 12 &&
           year >= currentYear)
        {
            try
            {
                var lastDayOfMonth = DateTime.DaysInMonth(year, month);
                var expiryDate = new DateTime(year, month, lastDayOfMonth);
                if (expiryDate < DateTime.UtcNow.Date)
                    errorList.Add("Card expiry must be in the future");
            }
            catch
            {
                errorList.Add("Invalid expiry date");
            }
        }

        // currency
        if (string.IsNullOrWhiteSpace(request.Currency) ||
        request.Currency.Length != 3 ||
        !_allowedCurrencies.Contains(request.Currency))
            errorList.Add("Invalid currency");

        // amount
        if (request.Amount <= 0)
            errorList.Add("Amount must be greater than 0");

        // cvv
        if (string.IsNullOrWhiteSpace(request.Cvv) ||
           (request.Cvv.Length != 3 && request.Cvv.Length != 4) ||
           !request.Cvv.All(char.IsDigit))
            errorList.Add("Invalid CVV");


        errors = errorList.ToArray();
        return errors.Length == 0;
    }

}