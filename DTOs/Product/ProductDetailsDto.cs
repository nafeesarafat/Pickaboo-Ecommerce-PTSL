namespace Ecommerce.Api.DTOs.Product;

/// <summary>
/// Detailed public representation of a single product. Adds the category and brand
/// descriptions that are useful on a product page without exposing any internal data.
/// </summary>
public class ProductDetailsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? BrandDescription { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
}