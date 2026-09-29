using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Helpers;

namespace Ecommerce.Api.DTOs.Auth;

public class ChangePasswordRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "CurrentPassword is required.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "NewPassword is required.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmNewPassword is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "NewPassword and ConfirmNewPassword do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (string failure in PasswordPolicy.Validate(NewPassword))
        {
            yield return new ValidationResult(failure, new[] { nameof(NewPassword) });
        }
    }
}
