using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsService _paymentsService;


    public PaymentsController(PaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost]
    // Behaviour: Anything involving bank comms will return 200 to merchant via this api, 503's etc from bank will be handled internally and only exposed to client via status ie authorised, declined, rejected
    public async Task<IActionResult> CreatePayment([FromBody] PostPaymentRequest request)
    {
        var response = await _paymentsService.ProcessPaymentAsync(request);

        // Only return 400 if it was rejected due to validation errors
        if (response.Status == PaymentStatus.Rejected && response.ValidationErrors?.Any() == true)
        {
            return BadRequest(new
            {
                paymentId = response.Id,
                errors = response.ValidationErrors
            });
        }

        // Otherwise return 200 OK with status (Authorized / Declined / Rejected by bank)
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsService.GetPayment(id);


        if (payment == null)
        {
            return NotFound();
        }

        return new OkObjectResult(payment);
    }
}