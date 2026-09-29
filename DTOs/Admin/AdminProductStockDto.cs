namespace Ecommerce.Api.DTOs.Admin;

public class AdminProductStockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
