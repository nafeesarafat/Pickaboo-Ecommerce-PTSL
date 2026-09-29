using Ecommerce.Api.Models;

namespace Ecommerce.Api.Services;

/// <summary>
/// Issues JWT access tokens. All signing material and lifetime come from <c>JwtOptions</c>;
/// nothing about the token is hard-coded here.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Creates a signed access token carrying the minimum identity claims needed by the API,
    /// together with the moment it stops being valid.
    /// </summary>
    JwtTokenResult GenerateAccessToken(UserInformation user, int? sessionId = null);
}

/// <summary>Result of <see cref="IJwtService.GenerateAccessToken"/>.</summary>
public record JwtTokenResult(string Token, DateTime ExpiresAtUtc);
