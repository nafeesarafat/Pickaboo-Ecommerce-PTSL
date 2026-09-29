namespace Ecommerce.Api.DTOs.Admin;

public class AdminBrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
