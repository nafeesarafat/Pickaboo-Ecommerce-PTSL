using Ecommerce.Api.DTOs.Order;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class OrderItemController : ControllerBase
{
    private readonly IOrderItemService _orderItemService;

    public OrderItemController(IOrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }

    [HttpGet("orders/{orderId:int}/items")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderItemResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderItemResponseDto>>>> GetOrderItems(
        [FromRoute] int orderId,
        CancellationToken cancellationToken)
    {
        var items = await _orderItemService.GetOrderItemsAsync(orderId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderItemResponseDto>>.Ok(items, "Order items retrieved successfully."));
    }

    [HttpGet("order-items/{orderItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderItemDetailsResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderItemDetailsResponseDto>>> GetOrderItem(
        [FromRoute] int orderItemId,
        CancellationToken cancellationToken)
    {
        var item = await _orderItemService.GetOrderItemAsync(orderItemId, cancellationToken);
        return Ok(ApiResponse<OrderItemDetailsResponseDto>.Ok(item, "Order item retrieved successfully."));
    }

    [HttpPost("orders/{orderId:int}/items")]
    [ProducesResponseType(typeof(ApiResponse<OrderItemDetailsResponseDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<OrderItemDetailsResponseDto>>> CreateOrderItem(
        [FromRoute] int orderId,
        [FromBody] CreateOrderItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var item = await _orderItemService.CreateOrderItemAsync(orderId, request, cancellationToken);
        return Created($"/api/order-items/{item.OrderItemId}",
            ApiResponse<OrderItemDetailsResponseDto>.Created(item, "Order item created successfully."));
    }

    [HttpPut("order-items/{orderItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderItemDetailsResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderItemDetailsResponseDto>>> UpdateOrderItem(
        [FromRoute] int orderItemId,
        [FromBody] UpdateOrderItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var item = await _orderItemService.UpdateOrderItemAsync(orderItemId, request, cancellationToken);
        return Ok(ApiResponse<OrderItemDetailsResponseDto>.Ok(item, "Order item updated successfully."));
    }

    [HttpDelete("order-items/{orderItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOrderItem(
        [FromRoute] int orderItemId,
        CancellationToken cancellationToken)
    {
        await _orderItemService.DeleteOrderItemAsync(orderItemId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Order item deleted successfully."));
    }
}