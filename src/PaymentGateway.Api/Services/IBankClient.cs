using System.Threading;
using System.Threading.Tasks;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public interface IBankClient
{
    Task<PaymentStatus> ProcessAsync(PostPaymentRequest request, CancellationToken cancellationToken = default);
}

