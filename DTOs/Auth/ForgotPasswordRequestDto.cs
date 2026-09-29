using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}
