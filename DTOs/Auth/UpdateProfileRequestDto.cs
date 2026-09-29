using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Auth;

/// <summary>
/// Only the profile fields the user is allowed to change are present, so security-sensitive
/// fields (Role, PasswordHash, UserId, IsActive, RegistrationDate) cannot be submitted.
/// </summary>
public class UpdateProfileRequestDto
{
    [Required(ErrorMessage = "UserName is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "UserName must be between 3 and 100 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "PhoneNumber must be at most 20 characters.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(255, ErrorMessage = "Address must be at most 255 characters.")]
    public string? Address { get; set; }

    [MaxLength(100, ErrorMessage = "City must be at most 100 characters.")]
    public string? City { get; set; }

    [MaxLength(100, ErrorMessage = "Country must be at most 100 characters.")]
    public string? Country { get; set; }
}
