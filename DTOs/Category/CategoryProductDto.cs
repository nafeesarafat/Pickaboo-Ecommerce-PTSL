namespace Ecommerce.Api.DTOs.Category;

public class CategoryProductDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
}
