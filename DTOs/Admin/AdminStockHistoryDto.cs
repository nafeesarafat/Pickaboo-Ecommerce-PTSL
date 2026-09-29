namespace Ecommerce.Api.DTOs.Admin;

public class AdminStockHistoryDto
{
    public int StockHistoryId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Change { get; set; }
    public string? Reason { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime Timestamp { get; set; }
}
