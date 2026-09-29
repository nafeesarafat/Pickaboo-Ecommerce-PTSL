namespace Ecommerce.Api.DTOs.Auth;

/// <summary>
/// Safe representation of a newly created account. No password or password hash is included,
/// and the role shown is the server-assigned default role.
/// </summary>
public class RegisterResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
}
