using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly IAdminNotificationService _service;

    public AdminNotificationsController(IAdminNotificationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] NotificationQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(await _service.GetAsync(query, cancellationToken), "Notifications retrieved."));

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] NotificationQueryDto query, CancellationToken cancellationToken)
    {
        query.HasOrder = true;
        var results = await _service.GetAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(results, "Order notifications retrieved."));
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] NotificationQueryDto query, CancellationToken cancellationToken)
    {
        query.HasUser = true;
        var results = await _service.GetAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(results, "Customer notifications retrieved."));
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock([FromQuery] NotificationQueryDto query, CancellationToken cancellationToken)
    {
        query.HasProduct = true;
        var results = await _service.GetAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(results, "Stock notifications retrieved."));
    }

    [HttpPost("promotional")]
    public async Task<IActionResult> SendPromotional(
        [FromBody] SendPromotionalNotificationRequestDto request,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<PromotionalNotificationResultDto>.Ok(
            await _service.SendPromotionAsync(request, cancellationToken),
            "Promotional notifications sent."));

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] NotificationSettingsDto request,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<NotificationSettingsDto>.Ok(
            await _service.UpdateSettingsAsync(request, cancellationToken),
            "Notification settings updated."));
}
