namespace Ecommerce.Api.DTOs.Admin;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public int OrderStatus { get; set; }
    public int TotalUsers { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}