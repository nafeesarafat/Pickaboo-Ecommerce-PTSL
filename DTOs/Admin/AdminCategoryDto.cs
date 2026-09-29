namespace Ecommerce.Api.DTOs.Admin;

public class AdminCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
    public bool IsActive { get; set; }
}
