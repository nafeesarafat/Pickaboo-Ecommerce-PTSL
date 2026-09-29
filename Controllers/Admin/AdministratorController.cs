using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController, Route("api/admin/administrators"), Authorize(Roles = "Admin")]
public class AdministratorController : ControllerBase
{
    private readonly IAdministratorService _service;
    public AdministratorController(IAdministratorService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdministratorQueryDto query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PagedResultDto<AdministratorDto>>.Ok(await _service.GetAllAsync(query, cancellationToken), "Administrators retrieved successfully."));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => Ok(ApiResponse<AdministratorDto>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Administrator retrieved successfully."));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdministratorRequestDto request, CancellationToken cancellationToken)
    {
        var administrator = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = administrator.AdministratorId }, ApiResponse<AdministratorDto>.Created(administrator, "Administrator created successfully."));
    }
}
