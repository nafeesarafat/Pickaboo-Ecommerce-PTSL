using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

/// <summary>
/// Single-row store configuration table (one settings record per store).
/// </summary>
public class StoreSettings
{
    public int SettingsId { get; set; }

    [Required]
    [MaxLength(30)]
    public string StoreKey { get; set; } = "default";

    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(50)]
    public string? Timezone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(5)]
    public string? CurrencyCode { get; set; }

    [MaxLength(10)]
    public string? Symbol { get; set; }

    public decimal DefaultShippingFee { get; set; }

    [MaxLength(255)]
    public string? SmtpHost { get; set; }

    public int? SmtpPort { get; set; }
}
