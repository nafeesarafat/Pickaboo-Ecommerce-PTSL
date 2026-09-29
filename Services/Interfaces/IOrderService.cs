using Ecommerce.Api.DTOs.Order;

namespace Ecommerce.Api.Services;

/// <summary>
/// Order business logic for roadmap APIs 23-27. Every authenticated operation resolves the
/// user from the JWT via <see cref="ICurrentUserService"/> and never trusts a client-supplied
/// user id. Product prices and stock are always read from the database.
/// </summary>
public interface IOrderService
{
    /// <summary>API 23 - places a new order by checking out the authenticated user's cart.</summary>
    Task<OrderResponseDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken);

    /// <summary>API 24 - returns all orders belonging to the authenticated user.</summary>
    Task<IReadOnlyList<OrderResponseDto>> GetMyOrdersAsync(CancellationToken cancellationToken);

    /// <summary>API 25 - returns the details of an order owned by the authenticated user.</summary>
    Task<OrderDetailsResponseDto> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken);

    /// <summary>API 26 - returns the status of an order owned by the authenticated user.</summary>
    Task<OrderStatusResponseDto> GetOrderStatusAsync(int orderId, CancellationToken cancellationToken);

    /// <summary>API 27 - cancels an order owned by the authenticated user.</summary>
    Task<OrderStatusResponseDto> CancelOrderAsync(int orderId, CancellationToken cancellationToken);
}
