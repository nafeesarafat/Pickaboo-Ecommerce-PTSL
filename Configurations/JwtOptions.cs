namespace Ecommerce.Api.Configurations;

/// <summary>
/// Bound from the "Jwt" section of appsettings.
/// Supply the values through environment variables (Jwt__Issuer, Jwt__Audience, Jwt__Key)
/// or user secrets so that no real secret is committed to source control.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
