using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Repositories;

namespace Ecommerce.Api.Services;

public class AdminInventoryService : IAdminInventoryService
{
    public const int DefaultLowStockThreshold = 5;

    private readonly IInventoryRepository _inventoryRepository;
    private readonly ICurrentUserService _currentUser;

    public AdminInventoryService(
        IInventoryRepository inventoryRepository,
        ICurrentUserService currentUser)
    {
        _inventoryRepository = inventoryRepository;
        _currentUser = currentUser;
    }

    public Task<AdminInventoryOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        return _inventoryRepository.GetOverviewAsync(DefaultLowStockThreshold, cancellationToken);
    }

    public async Task<AdminProductStockDto> GetProductStockAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        ValidateProductId(productId);
        return await _inventoryRepository.GetProductStockAsync(productId, cancellationToken)
            ?? throw new NotFoundException($"Product with id {productId} was not found.");
    }

    public async Task<IReadOnlyList<AdminLowStockProductDto>> GetLowStockProductsAsync(
        int threshold,
        CancellationToken cancellationToken)
    {
        ValidateThreshold(threshold);
        return await _inventoryRepository.GetLowStockProductsAsync(threshold, cancellationToken);
    }

    public Task<IReadOnlyList<AdminProductStockDto>> GetOutOfStockProductsAsync(
        CancellationToken cancellationToken)
    {
        return _inventoryRepository.GetOutOfStockProductsAsync(cancellationToken);
    }

    public async Task<AdminStockAdjustmentDto> AdjustStockAsync(
        int productId,
        AdjustStockRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateProductId(productId);
        if (request.Change == 0)
        {
            throw new BadRequestException("The stock adjustment must be non-zero.");
        }

        string reason = request.Reason.Trim();
        if (reason.Length == 0)
        {
            throw new BadRequestException("A reason for the stock adjustment is required.");
        }

        string updatedBy = _currentUser.GetRequiredUserId().ToString(
            System.Globalization.CultureInfo.InvariantCulture);
        var adjustment = await _inventoryRepository.AdjustStockAsync(
            productId,
            request.Change,
            reason,
            updatedBy,
            cancellationToken);

        if (adjustment is not null)
        {
            return adjustment;
        }

        if (!await _inventoryRepository.ProductExistsAsync(productId, cancellationToken))
        {
            throw new NotFoundException($"Product with id {productId} was not found.");
        }

        throw new BadRequestException("The stock adjustment would result in an invalid stock quantity.");
    }

    public async Task<IReadOnlyList<AdminStockHistoryDto>> GetStockHistoryAsync(
        int? productId,
        CancellationToken cancellationToken)
    {
        if (productId.HasValue)
        {
            ValidateProductId(productId.Value);
            if (!await _inventoryRepository.ProductExistsAsync(productId.Value, cancellationToken))
            {
                throw new NotFoundException($"Product with id {productId.Value} was not found.");
            }
        }

        return await _inventoryRepository.GetStockHistoryAsync(productId, cancellationToken);
    }

    private static void ValidateProductId(int productId)
    {
        if (productId <= 0)
        {
            throw new BadRequestException("A valid product id is required.");
        }
    }

    private static void ValidateThreshold(int threshold)
    {
        if (threshold < 0)
        {
            throw new BadRequestException("The low-stock threshold cannot be negative.");
        }
    }
}
