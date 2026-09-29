using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminOrderService
{
    Task<IReadOnlyList<AdminOrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminPendingOrderDto>> GetPendingOrdersAsync(CancellationToken cancellationToken);
    Task<AdminOrderDetailsDto> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken);
    Task<AdminOrderStatusDto> UpdateOrderStatusAsync(
        int orderId,
        UpdateAdminOrderStatusRequestDto request,
        CancellationToken cancellationToken);
    Task<AdminDeliveryStatusDto> UpdateDeliveryStatusAsync(
        int orderId,
        UpdateDeliveryStatusRequestDto request,
        CancellationToken cancellationToken);
    Task<AdminOrderCancelledDto> CancelOrderAsync(int orderId, CancellationToken cancellationToken);
}
