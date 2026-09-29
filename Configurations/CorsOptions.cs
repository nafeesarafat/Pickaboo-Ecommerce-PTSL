namespace Ecommerce.Api.Configurations;

/// <summary>
/// Bound from the "Cors" section of appsettings.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    public string PolicyName { get; set; } = "DefaultCorsPolicy";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
