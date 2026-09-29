using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Helpers;

namespace Ecommerce.Api.DTOs.Auth;

public class ResetPasswordRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Token is required.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "NewPassword is required.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPassword is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "NewPassword and ConfirmPassword do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (string failure in PasswordPolicy.Validate(NewPassword))
        {
            yield return new ValidationResult(failure, new[] { nameof(NewPassword) });
        }
    }
}
