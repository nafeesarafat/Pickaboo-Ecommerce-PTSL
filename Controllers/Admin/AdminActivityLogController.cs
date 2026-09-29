using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = "Admin")]
public class AdminActivityLogController : ControllerBase
{
    private readonly IAdminActivityLogService _service;

    public AdminActivityLogController(IAdminActivityLogService service) => _service = service;

    [HttpGet("login")]
    public async Task<IActionResult> GetLogin([FromQuery] LoginLogQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<LoginLogDto>>.Ok(await _service.GetLoginLogsAsync(query, cancellationToken), "Login logs retrieved."));

    [HttpGet("orders/{orderId:int}")]
    public async Task<IActionResult> GetOrderActivity(int orderId, [FromQuery] ActivityLogQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ActivityLogDto>>.Ok(await _service.GetOrderActivityAsync(orderId, query, cancellationToken), "Order activity retrieved."));

    [HttpGet("products/{productId:int}")]
    public async Task<IActionResult> GetProductActivity(int productId, [FromQuery] ActivityLogQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ActivityLogDto>>.Ok(await _service.GetProductActivityAsync(productId, query, cancellationToken), "Product activity retrieved."));

    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> GetUserActivity(int userId, [FromQuery] ActivityLogQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ActivityLogDto>>.Ok(await _service.GetUserActivityAsync(userId, query, cancellationToken), "User activity retrieved."));

    [HttpGet("system")]
    public async Task<IActionResult> GetSystem([FromQuery] SystemLogQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<SystemLogDto>>.Ok(await _service.GetSystemLogsAsync(query, cancellationToken), "System logs retrieved."));
}
