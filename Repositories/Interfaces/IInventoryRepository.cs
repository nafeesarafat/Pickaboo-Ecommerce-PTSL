using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Repositories;

public interface IInventoryRepository
{
    Task<AdminInventoryOverviewDto> GetOverviewAsync(int threshold, CancellationToken cancellationToken);
    Task<AdminProductStockDto?> GetProductStockAsync(int productId, CancellationToken cancellationToken);
    Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminLowStockProductDto>> GetLowStockProductsAsync(
        int threshold,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminProductStockDto>> GetOutOfStockProductsAsync(CancellationToken cancellationToken);
    Task<AdminStockAdjustmentDto?> AdjustStockAsync(
        int productId,
        int change,
        string reason,
        string updatedBy,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminStockHistoryDto>> GetStockHistoryAsync(
        int? productId,
        CancellationToken cancellationToken);
}
