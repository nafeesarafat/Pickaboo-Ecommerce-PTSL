namespace Ecommerce.Api.Configurations;

/// <summary>
/// Bound from the "PasswordReset" section of appsettings.
/// </summary>
public class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public int ExpiryMinutes { get; set; } = 15;

    public string ResetUrlBase { get; set; } = string.Empty;
}
