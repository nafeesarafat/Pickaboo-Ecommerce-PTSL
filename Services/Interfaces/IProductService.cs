using Ecommerce.Api.DTOs.Product;

namespace Ecommerce.Api.Services;

/// <summary>
/// Public product catalogue business logic for roadmap APIs 8-13.
/// Every method returns only active products and never exposes the raw EF entity.
/// </summary>
public interface IProductService
{
    /// <summary>API 8 - returns every active product available for public browsing.</summary>
    Task<IReadOnlyList<ProductListDto>> GetAllProductsAsync(CancellationToken cancellationToken);

    /// <summary>API 9 - returns a single active product by id, or throws if it is missing/inactive.</summary>
    Task<ProductDetailsDto> GetProductByIdAsync(int productId, CancellationToken cancellationToken);

    /// <summary>API 10 - case-insensitive product-name search across active products.</summary>
    Task<IReadOnlyList<ProductListDto>> SearchProductsAsync(string? query, CancellationToken cancellationToken);

    /// <summary>API 11 - returns active products flagged as featured.</summary>
    Task<IReadOnlyList<ProductListDto>> GetFeaturedProductsAsync(CancellationToken cancellationToken);

    /// <summary>API 12 - returns active products belonging to the given category.</summary>
    Task<IReadOnlyList<ProductListDto>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken);

    /// <summary>API 13 - returns active products belonging to the given brand.</summary>
    Task<IReadOnlyList<ProductListDto>> GetProductsByBrandAsync(int brandId, CancellationToken cancellationToken);
}