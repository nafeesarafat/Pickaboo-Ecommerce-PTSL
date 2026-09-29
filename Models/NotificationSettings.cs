using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

/// <summary>
/// Per-user notification preferences. UserId is nullable so that store-wide defaults can exist
/// before a user has saved their own preferences.
/// </summary>
public class NotificationSettings
{
    public int SettingsId { get; set; }

    public int? UserId { get; set; }

    public bool OrderAlerts { get; set; } = true;

    public bool StockAlerts { get; set; } = true;

    public bool PromotionalAlerts { get; set; } = true;

    [ForeignKey(nameof(UserId))]
    public UserInformation? User { get; set; }
}
