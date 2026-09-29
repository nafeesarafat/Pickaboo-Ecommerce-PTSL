using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private const string GenericPasswordResetMessage =
        "If an account exists for that email address, a password reset link has been sent.";

    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;
    private readonly IExtendedAuthService _extendedAuthService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUser,
        IExtendedAuthService extendedAuthService,
        IPasswordResetService passwordResetService)
    {
        _authService = authService;
        _currentUser = currentUser;
        _extendedAuthService = extendedAuthService;
        _passwordResetService = passwordResetService;
    }

    /// <summary>
    /// API 1 - Authenticates a user with email and password and returns a JWT access token.
    /// </summary>
    /// <response code="200">Login succeeded; the response contains the access token and safe user details.</response>
    /// <response code="400">The request body is missing or malformed.</response>
    /// <response code="401">The email or password is incorrect.</response>
    /// <response code="403">The account exists but has been deactivated.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        LoginResponseDto result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful."));
    }

    /// <summary>
    /// API 2 - Registers a new customer account. The role and all other security fields are set by the server.
    /// </summary>
    /// <response code="201">The account was created.</response>
    /// <response code="400">The request body is missing or fails validation.</response>
    /// <response code="409">The email address or username is already registered.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        RegisterResponseDto result = await _authService.RegisterAsync(request, cancellationToken);
        return Created(
            $"/api/auth/profile",
            ApiResponse<RegisterResponseDto>.Created(result, "Registration successful."));
    }

    /// <summary>
    /// API 3 - Deactivates the session associated with the authenticated JWT.
    /// Tokens issued before sessions were introduced remain valid until expiry because they
    /// do not contain a session claim.
    /// </summary>
    /// <response code="200">The session was deactivated.</response>
    /// <response code="401">No valid access token was supplied.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        await _extendedAuthService.EndSessionAsync(
            _currentUser.GetRequiredUserId(),
            _currentUser.SessionId,
            cancellationToken);
        return Ok(ApiResponse<object>.Ok(
            data: null,
            message: "Logged out. This session has been deactivated."));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<VerifyEmailResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VerifyEmailResultDto>>> VerifyEmail(
        [FromBody] VerifyEmailRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _extendedAuthService.VerifyEmailAsync(request, cancellationToken);
        return Ok(ApiResponse<VerifyEmailResultDto>.Ok(result, "Email verification completed."));
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TwoFactorStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TwoFactorStatusDto>>> UpdateTwoFactor(
        [FromBody] EnableTwoFactorRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _extendedAuthService.UpdateTwoFactorAsync(request, cancellationToken);
        return Ok(ApiResponse<TwoFactorStatusDto>.Ok(result, "Two-factor authentication settings updated."));
    }

    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserSessionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserSessionDto>>>> GetSessions(CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        var result = await _extendedAuthService.GetSessionsAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserSessionDto>>.Ok(result, "Active sessions retrieved."));
    }

    /// <summary>
    /// API 4 - Returns the profile of the currently authenticated user.
    /// The user is taken from the access token, not from the request.
    /// </summary>
    /// <response code="200">The authenticated user's profile.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The account referenced by the token no longer exists.</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProfileResponseDto>>> GetProfile(CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        ProfileResponseDto profile = await _authService.GetProfileAsync(userId, cancellationToken);
        return Ok(ApiResponse<ProfileResponseDto>.Ok(profile));
    }

    /// <summary>
    /// API 5 - Updates the editable profile fields of the currently authenticated user.
    /// </summary>
    /// <response code="200">The updated profile.</response>
    /// <response code="400">The request body is missing or fails validation.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The account referenced by the token no longer exists.</response>
    /// <response code="409">The new email address is already used by another account.</response>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ProfileResponseDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        ProfileResponseDto profile = await _authService.UpdateProfileAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<ProfileResponseDto>.Ok(profile, "Profile updated."));
    }

    /// <summary>
    /// API 6 - Changes the authenticated user's password after verifying the current one.
    /// </summary>
    /// <response code="200">The password was changed.</response>
    /// <response code="400">Validation failed or the current password is incorrect.</response>
    /// <response code="401">No valid access token was supplied.</response>
    /// <response code="404">The account referenced by the token no longer exists.</response>
    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        int userId = _currentUser.GetRequiredUserId();
        await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Password changed successfully."));
    }

    /// <summary>
    /// API 7 - Starts the password reset flow.
    /// The response is identical whether or not the email address is registered, so the endpoint
    /// cannot be used to discover which addresses have accounts.
    /// </summary>
    /// <response code="200">Always the same message, regardless of whether the account exists.</response>
    /// <response code="400">The request body is missing or malformed.</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, GenericPasswordResetMessage));
    }

    /// <summary>
    /// Resets a password using the raw token delivered by email. This endpoint is anonymous and
    /// does not require a JWT access token.
    /// </summary>
    /// <remarks>
    /// Returns 400 with a generic invalid/expired token message for invalid, expired, or used
    /// tokens. Password policy failures and mismatched confirmation also return 400.
    /// </remarks>
    /// <response code="200">The password was reset successfully.</response>
    /// <response code="400">The reset token is invalid or expired, or password validation failed.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _passwordResetService.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            request.ConfirmPassword,
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Password reset successfully."));
    }
}
