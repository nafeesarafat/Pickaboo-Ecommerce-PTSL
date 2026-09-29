using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Auth;

public class VerifyEmailRequestDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required, MaxLength(256)]
    public string Token { get; set; } = string.Empty;
}

public class VerifyEmailResultDto
{
    public int UserId { get; set; }
    public bool IsVerified { get; set; }
}

public class EnableTwoFactorRequestDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required, MaxLength(256)]
    public string Secret { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? QrCodeUrl { get; set; }

    public bool IsEnabled { get; set; }
}

public class TwoFactorStatusDto
{
    public bool IsEnabled { get; set; }
}

public class UserSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? Device { get; set; }
    public DateTime LoginDateTime { get; set; }
}

public class LoginActivityDto
{
    public int AdministratorId { get; set; }
    public string? AdministratorName { get; set; }
    public DateTime LoginDateTime { get; set; }
    public string? IpAddress { get; set; }
}
