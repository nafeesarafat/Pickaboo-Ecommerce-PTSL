namespace Ecommerce.Api.Services;

/// <summary>
/// Reads the identity of the caller from the validated JWT. Controllers use this instead of
/// repeating claim-extraction code, and a client-provided user id is never trusted.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    /// <summary>User id from the token, or null when the request is anonymous.</summary>
    int? UserId { get; }

    int? SessionId { get; }

    string? UserName { get; }

    string? Email { get; }

    string? Role { get; }

    /// <summary>
    /// Returns the authenticated user id.
    /// Throws <see cref="Helpers.AuthenticationFailedException"/> when the request is anonymous
    /// or the token does not carry a usable user id.
    /// </summary>
    int GetRequiredUserId();
}
