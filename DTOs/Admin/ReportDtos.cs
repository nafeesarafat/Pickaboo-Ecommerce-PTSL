using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class ReportDateRangeQueryDto
{
    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}

public class SalesReportDto : ReportDateRangeQueryDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
}

public class RevenueReportDto : ReportDateRangeQueryDto
{
    public decimal TotalRevenue { get; set; }
}

public class OrderReportDto : ReportDateRangeQueryDto
{
    public int TotalOrders { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
}

public class ProductReportDto : ReportDateRangeQueryDto
{
    public int ProductId { get; set; }
    public int UnitsSold { get; set; }
}

public class CustomerReportDto : ReportDateRangeQueryDto
{
    public int NewCustomers { get; set; }
    public int ActiveCustomers { get; set; }
}

public class InventoryReportDto
{
    public int TotalStock { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class PaymentReportDto : ReportDateRangeQueryDto
{
    public decimal TotalPayments { get; set; }
}

public class DiscountReportDto : ReportDateRangeQueryDto
{
    public decimal TotalDiscountAmount { get; set; }
    public int CouponsUsed { get; set; }
}

public class SalesByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
}

public class SalesByBrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
}

public class SalesByDateDto
{
    public DateTime Date { get; set; }
    public decimal TotalSales { get; set; }
}

public class ExportReportRequestDto : ReportDateRangeQueryDto
{
    [Required, MaxLength(50)]
    public string ReportType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Format { get; set; } = string.Empty;
}

public class ExportReportDto
{
    public string ReportType { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}

public class ReportExportRowDto
{
    public string Date { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}
