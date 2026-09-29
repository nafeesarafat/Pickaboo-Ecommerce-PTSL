using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminOrderController : ControllerBase
{
    private readonly IAdminOrderService _orderService;
    private readonly IAdminOrderQueryService _orderQueryService;

    public AdminOrderController(
        IAdminOrderService orderService,
        IAdminOrderQueryService orderQueryService)
    {
        _orderService = orderService;
        _orderQueryService = orderQueryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllOrdersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminOrderDto>>.Ok(orders, "Orders retrieved successfully."));
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminPendingOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetPendingOrdersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminPendingOrderDto>>.Ok(
            orders, "Pending orders retrieved successfully."));
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminOrderViewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus(
        [FromRoute, Required] string status,
        CancellationToken cancellationToken)
    {
        var orders = await _orderQueryService.GetByStatusAsync(status, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminOrderViewDto>>.Ok(
            orders, "Orders retrieved successfully."));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<AdminOrderViewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] AdminOrderSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderQueryService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<AdminOrderViewDto>>.Ok(
            orders, "Orders retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminOrderDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderDetailsAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminOrderDetailsDto>.Ok(order, "Order retrieved successfully."));
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<AdminOrderStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateAdminOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminOrderStatusDto>.Ok(order, "Order status updated successfully."));
    }

    [HttpPut("{id:int}/delivery-status")]
    [ProducesResponseType(typeof(ApiResponse<AdminDeliveryStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeliveryStatus(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateDeliveryStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.UpdateDeliveryStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminDeliveryStatusDto>.Ok(order, "Delivery status updated successfully."));
    }

    [HttpPut("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<AdminOrderCancelledDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.CancelOrderAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminOrderCancelledDto>.Ok(order, "Order cancelled successfully."));
    }
}
