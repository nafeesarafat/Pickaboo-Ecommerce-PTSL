namespace Ecommerce.Api.DTOs.Admin;

public class AdminInventoryOverviewDto
{
    public int TotalProducts { get; set; }
    public int InStock { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}
