using System.Security.Claims;
using Ecommerce.Api.Helpers;

namespace Ecommerce.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int? UserId
    {
        get
        {
            string? value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out int userId) ? userId : null;
        }
    }

    public int? SessionId
    {
        get
        {
            string? value = Principal?.FindFirstValue("sid");
            return int.TryParse(value, out int sessionId) ? sessionId : null;
        }
    }

    public string? UserName => Principal?.FindFirstValue(ClaimTypes.Name);

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public int GetRequiredUserId()
    {
        if (!IsAuthenticated)
        {
            throw new AuthenticationFailedException("Authentication is required.");
        }

        return UserId ?? throw new AuthenticationFailedException("The access token does not identify a user.");
    }
}
