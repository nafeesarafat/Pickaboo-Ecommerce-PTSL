namespace Ecommerce.Api.DTOs.Product;

/// <summary>
/// Public listing representation of a product. Returned by the browse, search,
/// category, brand and featured endpoints. Only fields useful to a customer are
/// exposed; internal database identifiers and unrelated relations are omitted.
/// </summary>
public class ProductListDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
}