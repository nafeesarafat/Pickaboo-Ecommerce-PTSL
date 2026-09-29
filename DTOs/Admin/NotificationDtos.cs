using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class NotificationQueryDto
{
    public int? NotificationId { get; set; }
    public string? Type { get; set; }
    public string? Message { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? OrderId { get; set; }
    public int? UserId { get; set; }
    public int? ProductId { get; set; }
    public bool? HasOrder { get; set; }
    public bool? HasUser { get; set; }
    public bool? HasProduct { get; set; }
}

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? OrderId { get; set; }
    public int? UserId { get; set; }
    public int? ProductId { get; set; }
}

public class SendPromotionalNotificationRequestDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string TargetAudience { get; set; } = string.Empty;
}

public class PromotionalNotificationResultDto
{
    public int NotificationsCreated { get; set; }
}

public class NotificationSettingsDto
{
    public bool OrderAlerts { get; set; }
    public bool StockAlerts { get; set; }
    public bool PromotionalAlerts { get; set; }
}
