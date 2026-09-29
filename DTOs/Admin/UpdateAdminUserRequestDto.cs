using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Admin;

public class UpdateAdminUserRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string RoleId { get; set; } = string.Empty;
}
