using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class GeneralStoreSettingsDto
{
    public string StoreName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Timezone { get; set; }
}

public class UpdateStoreInformationRequestDto
{
    [Required, MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }
}

public class UpdateContactInformationRequestDto
{
    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }
}

public class UpdateCurrencySettingsRequestDto
{
    [Required, MaxLength(5)]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;
}

public class UpdateShippingSettingsRequestDto
{
    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal DefaultShippingFee { get; set; }
}

public class UpdateEmailSettingsRequestDto
{
    [Required, MaxLength(255)]
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int SmtpPort { get; set; }
}

public class StoreSettingsUpdatedDto
{
    public bool Updated { get; set; }
}
