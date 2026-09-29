using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/customers")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminCustomerController : ControllerBase
{
    private readonly IAdminCustomerService _customerService;

    public AdminCustomerController(IAdminCustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminCustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        [FromBody] AddAdminCustomerRequestDto request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.AddAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AdminCustomerDto>.Created(customer, "Customer created successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminCustomerStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminCustomerStatusDto>.Ok(customer, "Customer deactivated successfully."));
    }

    [HttpGet("{id:int}/orders")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<AdminCustomerOrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrders(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var orders = await _customerService.GetOrdersAsync(id, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<AdminCustomerOrderDto>>.Ok(
            orders, "Customer orders retrieved successfully."));
    }

    [HttpGet("{id:int}/address")]
    [ProducesResponseType(typeof(ApiResponse<AdminCustomerAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddress(
        [FromRoute, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var address = await _customerService.GetAddressAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminCustomerAddressDto>.Ok(address, "Customer address retrieved successfully."));
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<AdminCustomerStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateAdminCustomerStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.UpdateStatusAsync(id, request.IsActive, cancellationToken);
        return Ok(ApiResponse<AdminCustomerStatusDto>.Ok(customer, "Customer status updated successfully."));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<AdminCustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] AdminCustomerSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        var customers = await _customerService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<AdminCustomerDto>>.Ok(
            customers, "Customers retrieved successfully."));
    }
}
