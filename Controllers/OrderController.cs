using Ecommerce.Api.DTOs.Order;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// API 23 - Places a new order by checking out the authenticated user's cart.
    /// The server is the authority for UserId (from JWT), product prices, stock,
    /// subtotal, shipping fee and final amount. All of these are calculated server-side.
    /// </summary>
    /// <param name="request">Checkout request. Client-supplied UserId, ProductId, StockQuantity,
    /// UnitPrice, SubTotal, ShippingFee and FinalAmount are ignored.</param>
    /// <response code="201">The order was created.</response>
    /// <response code="400">The cart is empty or required shipping fields are missing.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">A product in the cart no longer exists.</response>
    /// <response code="409">Requested quantity exceeds available stock.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> PlaceOrder(
        [FromBody] PlaceOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.PlaceOrderAsync(request, cancellationToken);
        return Created($"/api/orders/{order.OrderId}", ApiResponse<OrderResponseDto>.Created(order, "Order placed successfully."));
    }

    /// <summary>
    /// API 24 - Returns all orders belonging to the authenticated user. A user can never
    /// retrieve another user's orders.
    /// </summary>
    /// <response code="200">The authenticated user's orders.</response>
    /// <response code="401">No valid access token was supplied.</response>
    [HttpGet("my-orders")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderResponseDto>>>> GetMyOrders(
        CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetMyOrdersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderResponseDto>>.Ok(orders, "Orders retrieved successfully."));
    }

    /// <summary>
    /// API 25 - Returns the details of a specific order. The order is only returned if it
    /// belongs to the authenticated user.
    /// </summary>
    /// <param name="id">The order identifier from the route.</param>
    /// <response code="200">The order details.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The order does not exist for the authenticated user.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDetailsResponseDto>>> GetOrderDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderDetailsAsync(id, cancellationToken);
        return Ok(ApiResponse<OrderDetailsResponseDto>.Ok(order, "Order details retrieved successfully."));
    }

    /// <summary>
    /// API 26 - Returns the status of a specific order. The order is only returned if it
    /// belongs to the authenticated user.
    /// </summary>
    /// <param name="id">The order identifier from the route.</param>
    /// <response code="200">The order status.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The order does not exist for the authenticated user.</response>
    [HttpGet("{id:int}/status")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderStatusResponseDto>>> GetOrderStatus(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var status = await _orderService.GetOrderStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<OrderStatusResponseDto>.Ok(status, "Order status retrieved successfully."));
    }

    /// <summary>
    /// API 27 - Cancels an order owned by the authenticated user. Only orders with status
    /// "Pending" can be cancelled. The UserId from the request body is ignored; ownership is
    /// enforced via the JWT identity.
    /// </summary>
    /// <param name="id">The order identifier from the route.</param>
    /// <response code="200">The order was cancelled; the new status is returned.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The order does not exist for the authenticated user.</response>
    /// <response code="409">The order is in a status that cannot be cancelled (e.g. Delivered).</response>
    [HttpPut("{id:int}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<OrderStatusResponseDto>>> CancelOrder(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var status = await _orderService.CancelOrderAsync(id, cancellationToken);
        return Ok(ApiResponse<OrderStatusResponseDto>.Ok(status, "Order cancelled successfully."));
    }
}
