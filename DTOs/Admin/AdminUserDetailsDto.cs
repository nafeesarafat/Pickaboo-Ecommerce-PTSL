namespace Ecommerce.Api.DTOs.Admin;

public class AdminUserDetailsDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public DateTime RegistrationDate { get; set; }
}
