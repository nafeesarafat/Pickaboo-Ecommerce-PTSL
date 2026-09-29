using System.Linq.Expressions;
using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Product;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // Reusable projections. Keeping them as expression fields means every endpoint
    // projects only the columns the DTO needs, server-side, with no entity graph loaded.
    private static readonly Expression<Func<Product, ProductListDto>> ToListDto =
        p => new ProductListDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.CategoryName,
            BrandId = p.BrandId,
            BrandName = p.Brand.BrandName,
            StockQuantity = p.StockQuantity,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive
        };

    private static readonly Expression<Func<Product, ProductDetailsDto>> ToDetailsDto =
        p => new ProductDetailsDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.CategoryName,
            CategoryDescription = p.Category.Description,
            BrandId = p.BrandId,
            BrandName = p.Brand.BrandName,
            BrandDescription = p.Brand.Description,
            StockQuantity = p.StockQuantity,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive
        };

    // ------------------------------------------------------------------ API 8
    public async Task<IReadOnlyList<ProductListDto>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(ToListDto)
            .OrderBy(p => p.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ API 9
    public async Task<ProductDetailsDto> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new { p.ProductId, p.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {productId} was not found.");
        }

        if (!product.IsActive)
        {
            // Inactive products are not exposed through the public catalogue.
            throw new NotFoundException($"Product with id {productId} is not available.");
        }

        return await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(ToDetailsDto)
            .SingleAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ API 10
    public async Task<IReadOnlyList<ProductListDto>> SearchProductsAsync(string? query, CancellationToken cancellationToken)
    {
        string? escaped = NormalizeSearchQuery(query);

        if (escaped is null)
        {
            // An empty search returns the full active catalogue, matching the behaviour
            // expected by the roadmap for an empty search parameter.
            return await GetAllProductsAsync(cancellationToken);
        }

        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && EF.Functions.Like(p.ProductName, escaped, "!"))
            .Select(ToListDto)
            .OrderBy(p => p.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ API 11
    public async Task<IReadOnlyList<ProductListDto>> GetFeaturedProductsAsync(CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsFeatured)
            .Select(ToListDto)
            .OrderBy(p => p.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ API 12
    public async Task<IReadOnlyList<ProductListDto>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken)
    {
        if (categoryId <= 0)
        {
            throw new BadRequestException("A valid category id is required.");
        }

        return await CategoryProductQuery.GetActiveProducts(_context, categoryId)
            .Select(ToListDto)
            .OrderBy(p => p.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ API 13
    public async Task<IReadOnlyList<ProductListDto>> GetProductsByBrandAsync(int brandId, CancellationToken cancellationToken)
    {
        if (brandId <= 0)
        {
            throw new BadRequestException("A valid brand id is required.");
        }

        return await _context.Products
            .AsNoTracking()
            .Where(p => p.BrandId == brandId && p.IsActive)
            .Select(ToListDto)
            .OrderBy(p => p.ProductName)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Normalises a search query into a SQL LIKE pattern.
    /// Returns null when the query is empty or whitespace, signalling "match everything".
    /// LIKE wildcard characters in the caller input are escaped so a literal "%" or "_"
    /// is searched for verbatim rather than acting as a wildcard.
    /// </summary>
    private static string? NormalizeSearchQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        string trimmed = query.Trim();

        // Escape LIKE special characters. "!" is used as the single-character escape marker.
        return "%" + trimmed
            .Replace("!", "!!")
            .Replace("%", "!%")
            .Replace("_", "!_")
            .Replace("[", "![")
            .Replace("]", "!]") + "%";
    }
}