namespace Ecommerce.Api.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    /// <summary>UTC instant at which the access token stops being accepted.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Lifetime of the access token.</summary>
    public int ExpiresInMinutes { get; set; }

    public ProfileResponseDto User { get; set; } = null!;
}
