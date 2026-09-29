using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "Admin")]
public class AdminStoreSettingsController : ControllerBase
{
    private readonly IStoreSettingsService _service;

    public AdminStoreSettingsController(IStoreSettingsService service) => _service = service;

    [HttpGet("general")]
    public async Task<IActionResult> GetGeneral(CancellationToken cancellationToken) =>
        Ok(ApiResponse<GeneralStoreSettingsDto>.Ok(await _service.GetGeneralAsync(cancellationToken), "General settings retrieved."));

    [HttpPut("store")]
    public async Task<IActionResult> UpdateStore([FromBody] UpdateStoreInformationRequestDto request, CancellationToken cancellationToken)
    {
        await _service.UpdateStoreAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Store information updated."));
    }

    [HttpPut("contact")]
    public async Task<IActionResult> UpdateContact([FromBody] UpdateContactInformationRequestDto request, CancellationToken cancellationToken)
    {
        await _service.UpdateContactAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Contact information updated."));
    }

    [HttpPut("currency")]
    public async Task<IActionResult> UpdateCurrency([FromBody] UpdateCurrencySettingsRequestDto request, CancellationToken cancellationToken)
    {
        await _service.UpdateCurrencyAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Currency settings updated."));
    }

    [HttpPut("shipping")]
    public async Task<IActionResult> UpdateShipping([FromBody] UpdateShippingSettingsRequestDto request, CancellationToken cancellationToken)
    {
        await _service.UpdateShippingAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Shipping settings updated."));
    }

    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailSettingsRequestDto request, CancellationToken cancellationToken)
    {
        await _service.UpdateEmailAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Email settings updated."));
    }
}
