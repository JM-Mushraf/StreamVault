using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Service.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectFileStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Subscription endpoints: create subscriptions and retrieve active subscriptions.
/// </summary>
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [Authorize(Roles = "1")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var subs = await _subscriptionService.GetActiveSubscriptionsAsync();
        return Ok(subs);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SV.Common.DTOs.SubscriptionCreateRequest request)
    {
        string transactionRef = Guid.NewGuid().ToString();

        var userGuid = User.FindFirst("UserGuid")?.Value;
        if (string.IsNullOrWhiteSpace(userGuid)) return Unauthorized();

        var userStore = HttpContext.RequestServices.GetRequiredService<SV.Store.Abstractions.IUserStore>();
        var userId = await userStore.GetUserIdByGuidAsync(userGuid);
        if (userId == null) return NotFound(new { success = false, message = "User not found" });

        await _subscriptionService.CreateSubscriptionAsync(userId.Value, request.PlanId, request.StartDate, request.EndDate, "PENDING", transactionRef);
        return Ok(new { success = true, transactionReference = transactionRef });
    }
}
