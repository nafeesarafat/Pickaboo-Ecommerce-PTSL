namespace Ecommerce.Api.DTOs.Admin;

public class AdminUserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? LastLoginDateTime { get; set; }
    public bool IsActive { get; set; }
    public string RoleId { get; set; } = string.Empty;
}
