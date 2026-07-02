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

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var history = await _paymentService.GetPaymentHistoryAsync(userGuid);
        return Ok(history);
    }

    [Authorize]
    [HttpGet("invoice/{paymentGuid}")]
    public async Task<IActionResult> DownloadInvoice(string paymentGuid)
    {
        dynamic? receipt = await _paymentService.GetReceiptDetailsAsync(paymentGuid);
        if (receipt == null)
        {
            return NotFound(new { error = "Invoice not found or user is unauthorized to download this invoice." });
        }

        var userGuid = User.FindFirst("UserGuid")?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (userRole != "1" && !string.Equals(userGuid, (string)receipt.UserGuid, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var pdfBytes = ProjectFileStructure.Helpers.InvoicePdfGenerator.GenerateInvoice(receipt);
        return File(pdfBytes, "application/pdf", $"Invoice-{paymentGuid}.pdf");
    }
}
