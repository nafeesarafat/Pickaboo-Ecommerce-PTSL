using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminProductService : IAdminProductService
{
    private readonly AppDbContext _context;

    public AdminProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminProductResponseDto>> GetAllProductsAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Select(product => new AdminProductResponseDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.CategoryName,
                BrandId = product.BrandId,
                BrandName = product.Brand.BrandName,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                ImageUrl = product.ImageUrl
            })
            .OrderBy(product => product.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminProductDetailsDto> GetProductDetailsAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(item => item.ProductId == productId)
            .Select(item => new AdminProductDetailsDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Description = item.Description,
                Price = item.Price,
                StockQuantity = item.StockQuantity,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.CategoryName,
                BrandId = item.BrandId,
                BrandName = item.Brand.BrandName,
                IsFeatured = item.IsFeatured,
                IsActive = item.IsActive,
                ImageUrl = item.ImageUrl,
                CreatedDate = item.CreatedDate,
                UpdatedDate = item.UpdatedDate
            })
            .SingleOrDefaultAsync(cancellationToken);

        return product ?? throw new NotFoundException($"Product with id {productId} was not found.");
    }

    public async Task<AdminProductResponseDto> AddProductAsync(
        AddProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.CategoryId == request.CategoryId, cancellationToken);
        if (category is null)
        {
            throw new BadRequestException("The specified category does not exist.");
        }

        var brand = await _context.Brands
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.BrandId == request.BrandId, cancellationToken);
        if (brand is null)
        {
            throw new BadRequestException("The specified brand does not exist.");
        }

        ValidateReferenceName(request.CategoryName, category.CategoryName, "category");
        ValidateReferenceName(request.BrandName, brand.BrandName, "brand");

        var now = DateTime.UtcNow;
        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            IsFeatured = request.IsFeatured,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl,
            StockQuantity = request.StockQuantity,
            BrandId = request.BrandId,
            IsActive = request.IsActive,
            CreatedDate = now,
            UpdatedDate = now
        };

        _context.Products.Add(product);
        _context.ActivityLogs.Add(new ActivityLog
        {
            Product = product,
            Action = "Created",
            Description = $"Product '{product.ProductName}' was created.",
            Timestamp = now
        });
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(product, category.CategoryName, brand.BrandName);
    }

    public async Task<AdminProductResponseDto> UpdateProductAsync(
        int productId,
        UpdateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .SingleOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"Product with id {productId} was not found.");
        }

        var category = await _context.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.CategoryId == request.CategoryId, cancellationToken);
        if (category is null)
        {
            throw new BadRequestException("The specified category does not exist.");
        }

        var brand = await _context.Brands
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.BrandId == request.BrandId, cancellationToken);
        if (brand is null)
        {
            throw new BadRequestException("The specified brand does not exist.");
        }

        product.ProductName = request.ProductName.Trim();
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.ImageUrl = request.ImageUrl;
        product.StockQuantity = request.StockQuantity;
        product.Price = request.Price;
        product.IsFeatured = request.IsFeatured;
        product.IsActive = request.IsActive;
        product.UpdatedDate = DateTime.UtcNow;

        _context.ActivityLogs.Add(new ActivityLog
        {
            ProductId = product.ProductId,
            Action = "Updated",
            Description = $"Product '{product.ProductName}' was updated.",
            Timestamp = product.UpdatedDate
        });
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(product, category.CategoryName, brand.BrandName);
    }

    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .SingleOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"Product with id {productId} was not found.");
        }

        product.IsActive = false;
        product.UpdatedDate = DateTime.UtcNow;
        _context.ActivityLogs.Add(new ActivityLog
        {
            ProductId = product.ProductId,
            Action = "Deactivated",
            Description = $"Product '{product.ProductName}' was deactivated.",
            Timestamp = product.UpdatedDate
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static AdminProductResponseDto ToResponse(Product product, string categoryName, string brandName)
    {
        return new AdminProductResponseDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            BrandId = product.BrandId,
            BrandName = brandName,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl
        };
    }

    private static void ValidateReferenceName(string? requestedName, string actualName, string referenceType)
    {
        if (!string.IsNullOrWhiteSpace(requestedName)
            && !string.Equals(requestedName.Trim(), actualName, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException($"The supplied {referenceType} name does not match its id.");
        }
    }
}