using Ecommerce.Api.DTOs.Auth;

namespace Ecommerce.Api.Services;

/// <summary>
/// Authentication business logic for roadmap APIs 1-7.
/// Every method identifies the user from the authenticated token, never from client input.
/// </summary>
public interface IAuthService
{
    /// <summary>API 1 - verifies credentials and issues a JWT access token.</summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);

    /// <summary>API 2 - creates a customer account with a server-assigned role.</summary>
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);

    /// <summary>API 4 - returns the profile of the authenticated user.</summary>
    Task<ProfileResponseDto> GetProfileAsync(int userId, CancellationToken cancellationToken);

    /// <summary>API 5 - updates the editable profile fields of the authenticated user.</summary>
    Task<ProfileResponseDto> UpdateProfileAsync(
        int userId,
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken);

    /// <summary>API 6 - replaces the authenticated user's password after verifying the current one.</summary>
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken);

    /// <summary>API 7 - starts the password reset flow without revealing whether the account exists.</summary>
    Task RequestPasswordResetAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken);
}
