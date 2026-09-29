using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminInventoryService
{
    Task<AdminInventoryOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<AdminProductStockDto> GetProductStockAsync(int productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminLowStockProductDto>> GetLowStockProductsAsync(
        int threshold,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminProductStockDto>> GetOutOfStockProductsAsync(CancellationToken cancellationToken);
    Task<AdminStockAdjustmentDto> AdjustStockAsync(
        int productId,
        AdjustStockRequestDto request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminStockHistoryDto>> GetStockHistoryAsync(
        int? productId,
        CancellationToken cancellationToken);
}
