using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class AdministratorQueryDto
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    public int? AdministratorId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Username { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? AccountStatus { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class CreateAdministratorRequestDto
{
    [Required, MaxLength(200)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = string.Empty;
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [Required, MaxLength(100)] public string Username { get; set; } = string.Empty;
    [Required, MinLength(8), MaxLength(128)] public string Password { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int RoleId { get; set; }
    public string? RoleName { get; set; }
    [Required, MaxLength(20)] public string AccountStatus { get; set; } = "Active";
}

public class AdministratorDto
{
    public int AdministratorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class AuditLogQueryDto
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    public int? AuditLogId { get; set; }
    public int? AdministratorId { get; set; }
    public string? AdministratorName { get; set; }
    public string? Action { get; set; }
    public string? Description { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class AuditLogDto
{
    public int AuditLogId { get; set; }
    public int AdministratorId { get; set; }
    public string AdministratorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int Threshold { get; set; }
}

public class RecentOrderDto
{
    public int OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public class SalesOverviewDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
}

public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}
