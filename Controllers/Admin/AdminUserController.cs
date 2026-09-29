using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _userService;

    public AdminUserController(IAdminUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminUserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminUserDto>>.Ok(users, "Users retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserDetailsAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminUserDetailsDto>.Ok(user, "User retrieved successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateAdminUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminUserDto>.Ok(user, "User updated successfully."));
    }
}
