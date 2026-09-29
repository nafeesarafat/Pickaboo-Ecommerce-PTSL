using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/payment-methods")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminPaymentMethodController : ControllerBase
{
    private readonly IAdminPaymentMethodService _paymentMethodService;

    public AdminPaymentMethodController(IAdminPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminPaymentMethodDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var methods = await _paymentMethodService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminPaymentMethodDto>>.Ok(
            methods, "Payment methods retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminPaymentMethodDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        [FromBody] PaymentMethodRequestDto request,
        CancellationToken cancellationToken)
    {
        var method = await _paymentMethodService.AddAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AdminPaymentMethodDto>.Created(method, "Payment method created successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminPaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] PaymentMethodRequestDto request,
        CancellationToken cancellationToken)
    {
        var method = await _paymentMethodService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminPaymentMethodDto>.Ok(method, "Payment method updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _paymentMethodService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
