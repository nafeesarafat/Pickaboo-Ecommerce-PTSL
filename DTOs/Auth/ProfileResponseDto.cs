namespace Ecommerce.Api.DTOs.Auth;

/// <summary>
/// Safe profile representation returned by the profile endpoints.
/// PasswordHash is deliberately absent.
/// </summary>
public class ProfileResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
}
