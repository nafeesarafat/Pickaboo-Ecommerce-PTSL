using Ecommerce.Api.DTOs.Order;

namespace Ecommerce.Api.Services;

public interface IOrderItemService
{
    Task<IReadOnlyList<OrderItemResponseDto>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken);
    Task<OrderItemDetailsResponseDto> GetOrderItemAsync(int orderItemId, CancellationToken cancellationToken);
    Task<OrderItemDetailsResponseDto> CreateOrderItemAsync(int orderId, CreateOrderItemRequestDto request, CancellationToken cancellationToken);
    Task<OrderItemDetailsResponseDto> UpdateOrderItemAsync(int orderItemId, UpdateOrderItemRequestDto request, CancellationToken cancellationToken);
    Task DeleteOrderItemAsync(int orderItemId, CancellationToken cancellationToken);
}