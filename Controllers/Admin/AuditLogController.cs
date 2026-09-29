using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController, Route("api/admin/audit-logs"), Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _service;
    public AuditLogController(IAuditLogService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogQueryDto query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PagedResultDto<AuditLogDto>>.Ok(await _service.GetAllAsync(query, cancellationToken), "Audit logs retrieved successfully."));
}
