using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;
// this is an in memory repository, storage only no business logic
// we store our own internal payment model with full info, not exposed to merchant
public class PaymentsRepository
{
    private readonly List<InternalPayment> _payments = new();

    public void Add(InternalPayment payment) => _payments.Add(payment);

    public InternalPayment Get(Guid id) => _payments.FirstOrDefault(p => p.Id == id);
}