using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/login-activity")]
[Authorize(Roles = "Admin")]
public class AdminLoginActivityController : ControllerBase
{
    private readonly IExtendedAuthService _service;

    public AdminLoginActivityController(IExtendedAuthService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<LoginActivityDto>>.Ok(
            await _service.GetLoginActivityAsync(cancellationToken),
            "Administrator login activity retrieved."));
}
