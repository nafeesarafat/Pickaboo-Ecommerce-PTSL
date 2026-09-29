using Ecommerce.Api.DTOs.Auth;

namespace Ecommerce.Api.Services;

public interface IExtendedAuthService
{
    Task<VerifyEmailResultDto> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken);
    Task<TwoFactorStatusDto> UpdateTwoFactorAsync(EnableTwoFactorRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSessionDto>> GetSessionsAsync(int userId, CancellationToken cancellationToken);
    Task EndSessionAsync(int userId, int? sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<LoginActivityDto>> GetLoginActivityAsync(CancellationToken cancellationToken);
}
