using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IStoreSettingsService
{
    Task<GeneralStoreSettingsDto> GetGeneralAsync(CancellationToken cancellationToken);
    Task UpdateStoreAsync(UpdateStoreInformationRequestDto request, CancellationToken cancellationToken);
    Task UpdateContactAsync(UpdateContactInformationRequestDto request, CancellationToken cancellationToken);
    Task UpdateCurrencyAsync(UpdateCurrencySettingsRequestDto request, CancellationToken cancellationToken);
    Task UpdateShippingAsync(UpdateShippingSettingsRequestDto request, CancellationToken cancellationToken);
    Task UpdateEmailAsync(UpdateEmailSettingsRequestDto request, CancellationToken cancellationToken);
}
