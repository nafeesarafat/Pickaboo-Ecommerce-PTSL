using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// API 8 - Returns every active product available for public browsing.
    /// </summary>
    /// <response code="200">The active product catalogue.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllProductsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>.Ok(
            products, "Products retrieved successfully."));
    }

    /// <summary>
    /// API 9 - Returns a single active product by id.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <response code="200">The requested product.</response>
    /// <response code="400">The id is not a positive integer.</response>
    /// <response code="404">No active product exists with the given id.</response>
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<DTOs.Product.ProductDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        [FromRoute] int productId,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(productId, cancellationToken);
        return Ok(ApiResponse<DTOs.Product.ProductDetailsDto>.Ok(
            product, "Product retrieved successfully."));
    }

    /// <summary>
    /// API 10 - Case-insensitive product-name search. An empty or whitespace query
    /// returns the full active catalogue.
    /// </summary>
    /// <param name="q">The search term to match against the product name.</param>
    /// <response code="200">The matching products.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var products = await _productService.SearchProductsAsync(q, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>.Ok(
            products, "Search completed."));
    }

    /// <summary>
    /// API 11 - Returns active products flagged as featured.
    /// </summary>
    /// <response code="200">The featured products.</response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured(CancellationToken cancellationToken)
    {
        var products = await _productService.GetFeaturedProductsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>.Ok(
            products, "Featured products retrieved successfully."));
    }

    /// <summary>
    /// API 12 - Returns active products belonging to the given category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <response code="200">The products in the category (possibly an empty list).</response>
    /// <response code="400">The id is not a positive integer.</response>
    [HttpGet("category/{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCategory(
        [FromRoute] int categoryId,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>.Ok(
            products, "Category products retrieved successfully."));
    }

    /// <summary>
    /// API 13 - Returns active products belonging to the given brand.
    /// </summary>
    /// <param name="brandId">The brand identifier.</param>
    /// <response code="200">The products for the brand (possibly an empty list).</response>
    /// <response code="400">The id is not a positive integer.</response>
    [HttpGet("brand/{brandId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByBrand(
        [FromRoute] int brandId,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsByBrandAsync(brandId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DTOs.Product.ProductListDto>>.Ok(
            products, "Brand products retrieved successfully."));
    }
}