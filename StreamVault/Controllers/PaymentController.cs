using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Payment endpoints: record payment transactions.
/// </summary>
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SV.Common.DTOs.PaymentCreateRequest request)
    {
        await _paymentService.InsertPaymentAsync(request.SubscriptionId, request.Amount, request.PaymentMethod, request.TransactionStatus, request.PaidOn);
        return Ok(new { success = true });
    }
}
