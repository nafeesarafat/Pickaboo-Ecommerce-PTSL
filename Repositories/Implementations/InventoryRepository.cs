using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminInventoryOverviewDto> GetOverviewAsync(
        int threshold,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(products => new AdminInventoryOverviewDto
            {
                TotalProducts = products.Count(),
                InStock = products.Count(product => product.StockQuantity > threshold),
                LowStock = products.Count(product =>
                    product.StockQuantity > 0 && product.StockQuantity <= threshold),
                OutOfStock = products.Count(product => product.StockQuantity == 0)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new AdminInventoryOverviewDto();
    }

    public Task<AdminProductStockDto?> GetProductStockAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        return _context.Products
            .AsNoTracking()
            .Where(product => product.ProductId == productId)
            .Select(product => new AdminProductStockDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                StockQuantity = product.StockQuantity
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken)
    {
        return _context.Products.AnyAsync(product => product.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminLowStockProductDto>> GetLowStockProductsAsync(
        int threshold,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(product => product.StockQuantity > 0 && product.StockQuantity <= threshold)
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.ProductName)
            .Select(product => new AdminLowStockProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                StockQuantity = product.StockQuantity,
                Threshold = threshold
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminProductStockDto>> GetOutOfStockProductsAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(product => product.StockQuantity == 0)
            .OrderBy(product => product.ProductName)
            .Select(product => new AdminProductStockDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                StockQuantity = product.StockQuantity
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminStockAdjustmentDto?> AdjustStockAsync(
        int productId,
        int change,
        string reason,
        string updatedBy,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        DateTime timestamp = DateTime.UtcNow;

        int updatedRows = await _context.Products
            .Where(product => product.ProductId == productId
                && (long)product.StockQuantity + change >= 0
                && (long)product.StockQuantity + change <= int.MaxValue)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(product => product.StockQuantity, product => product.StockQuantity + change)
                    .SetProperty(product => product.UpdatedDate, timestamp),
                cancellationToken);

        if (updatedRows == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var productInfo = await _context.Products
            .AsNoTracking()
            .Where(product => product.ProductId == productId)
            .Select(product => new { product.ProductName, product.StockQuantity })
            .SingleAsync(cancellationToken);

        var history = new StockHistory
        {
            ProductId = productId,
            Change = change,
            Reason = reason,
            UpdatedBy = updatedBy,
            Timestamp = timestamp
        };

        _context.StockHistories.Add(history);
        _context.ActivityLogs.Add(new ActivityLog
        {
            ProductId = productId,
            Action = "Stock adjusted",
            Description = $"Stock changed by {change}. Reason: {reason}",
            Timestamp = timestamp
        });

        var notificationSettings = await _context.NotificationSettings
            .AsNoTracking()
            .Where(settings => settings.UserId == null)
            .OrderBy(settings => settings.SettingsId)
            .FirstOrDefaultAsync(cancellationToken);
        if ((notificationSettings?.StockAlerts ?? true)
            && productInfo.StockQuantity <= AdminInventoryService.DefaultLowStockThreshold)
        {
            _context.Notifications.Add(new Notification
            {
                Type = "Stock",
                Message = productInfo.StockQuantity == 0
                    ? $"Product '{productInfo.ProductName}' is out of stock."
                    : $"Product '{productInfo.ProductName}' is low on stock ({productInfo.StockQuantity} remaining).",
                ProductId = productId,
                CreatedDate = timestamp
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AdminStockAdjustmentDto
        {
            StockHistoryId = history.StockHistoryId,
            ProductId = productId,
            ProductName = productInfo.ProductName,
            Change = history.Change,
            Reason = history.Reason,
            UpdatedBy = history.UpdatedBy,
            Timestamp = history.Timestamp,
            StockQuantity = productInfo.StockQuantity
        };
    }

    public async Task<IReadOnlyList<AdminStockHistoryDto>> GetStockHistoryAsync(
        int? productId,
        CancellationToken cancellationToken)
    {
        var history = _context.StockHistories.AsNoTracking().AsQueryable();
        if (productId.HasValue)
        {
            history = history.Where(item => item.ProductId == productId.Value);
        }

        return await history
            .OrderByDescending(item => item.Timestamp)
            .ThenByDescending(item => item.StockHistoryId)
            .Select(item => new AdminStockHistoryDto
            {
                StockHistoryId = item.StockHistoryId,
                ProductId = item.ProductId,
                ProductName = item.Product.ProductName,
                Change = item.Change,
                Reason = item.Reason,
                UpdatedBy = item.UpdatedBy,
                Timestamp = item.Timestamp
            })
            .ToArrayAsync(cancellationToken);
    }
}
