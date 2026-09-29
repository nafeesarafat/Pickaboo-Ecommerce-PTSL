using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminShippingService
{
    Task<IReadOnlyList<AdminShippingMethodDto>> GetMethodsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminShippingZoneDto>> GetZonesAsync(CancellationToken cancellationToken);
    Task<AdminShippingChargeDto> GetZoneChargeAsync(int zoneId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminDeliveryAreaDto>> GetDeliveryAreasAsync(CancellationToken cancellationToken);
}
