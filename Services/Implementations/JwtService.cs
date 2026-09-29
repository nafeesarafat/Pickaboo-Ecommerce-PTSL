using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Api.Configurations;
using Ecommerce.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Api.Services;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public JwtTokenResult GenerateAccessToken(UserInformation user, int? sessionId = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException(
                "Jwt:Key is not configured, so a signing key cannot be created. Set Jwt__Key.");
        }

        // Only the identity claims the API actually needs are included. No password, no token, no ids
        // that are not already visible to the owner of the account.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        if (sessionId.HasValue)
        {
            claims.Add(new Claim("sid", sessionId.Value.ToString()));
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256));

        return new JwtTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
