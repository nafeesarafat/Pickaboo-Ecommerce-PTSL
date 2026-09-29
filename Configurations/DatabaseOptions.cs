namespace Ecommerce.Api.Configurations;

/// <summary>
/// Bound from the "ConnectionStrings" section of appsettings.
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string DefaultConnection { get; set; } = string.Empty;
}
