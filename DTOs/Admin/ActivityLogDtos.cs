using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class ActivityLogQueryDto
{
    public int? LogId { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public string? Description { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class ActivityLogDto
{
    public int LogId { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
}

public class LoginLogQueryDto
{
    public int? LogId { get; set; }
    public string? AdministratorName { get; set; }
    public DateTime? LoginDateTime { get; set; }
    public string? Status { get; set; }
}

public class LoginLogDto
{
    public int LogId { get; set; }
    public string? AdministratorName { get; set; }
    public DateTime LoginDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SystemLogQueryDto
{
    public int? LogId { get; set; }
    public string? Level { get; set; }
    public string? Message { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class SystemLogDto
{
    public int LogId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
