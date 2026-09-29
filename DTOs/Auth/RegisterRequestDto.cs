using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Helpers;

namespace Ecommerce.Api.DTOs.Auth;

public class RegisterRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "UserName is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "UserName must be between 3 and 100 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPassword is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and ConfirmPassword do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "PhoneNumber must be at most 20 characters.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(255, ErrorMessage = "Address must be at most 255 characters.")]
    public string? Address { get; set; }

    [MaxLength(100, ErrorMessage = "City must be at most 100 characters.")]
    public string? City { get; set; }

    [MaxLength(100, ErrorMessage = "Country must be at most 100 characters.")]
    public string? Country { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (string failure in PasswordPolicy.Validate(Password))
        {
            yield return new ValidationResult(failure, new[] { nameof(Password) });
        }
    }
}
