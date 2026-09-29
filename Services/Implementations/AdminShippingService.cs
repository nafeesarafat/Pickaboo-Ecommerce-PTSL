using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminShippingService : IAdminShippingService
{
    private readonly AppDbContext _context;

    public AdminShippingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminShippingMethodDto>> GetMethodsAsync(
        CancellationToken cancellationToken)
    {
        return await _context.ShippingMethods
            .AsNoTracking()
            .OrderBy(method => method.ShippingMethodId)
            .Select(method => new AdminShippingMethodDto
            {
                ShippingMethodId = method.ShippingMethodId,
                Name = method.Name,
                IsActive = method.IsActive
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminShippingZoneDto>> GetZonesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.ShippingZones
            .AsNoTracking()
            .OrderBy(zone => zone.ZoneId)
            .Select(zone => new AdminShippingZoneDto
            {
                ZoneId = zone.ZoneId,
                ZoneName = zone.ZoneName
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminShippingChargeDto> GetZoneChargeAsync(
        int zoneId,
        CancellationToken cancellationToken)
    {
        ValidateZoneId(zoneId);
        return await _context.ShippingZones
            .AsNoTracking()
            .Where(zone => zone.ZoneId == zoneId)
            .Select(zone => new AdminShippingChargeDto
            {
                ZoneId = zone.ZoneId,
                ZoneName = zone.ZoneName,
                Charge = zone.Charge
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Shipping zone with id {zoneId} was not found.");
    }

    public async Task<IReadOnlyList<AdminDeliveryAreaDto>> GetDeliveryAreasAsync(
        CancellationToken cancellationToken)
    {
        return await _context.DeliveryAreas
            .AsNoTracking()
            .OrderBy(area => area.AreaId)
            .Select(area => new AdminDeliveryAreaDto
            {
                AreaId = area.AreaId,
                AreaName = area.AreaName,
                ZoneId = area.ZoneId,
                ZoneName = area.Zone.ZoneName
            })
            .ToArrayAsync(cancellationToken);
    }

    private static void ValidateZoneId(int zoneId)
    {
        if (zoneId <= 0)
        {
            throw new BadRequestException("A valid shipping zone id is required.");
        }
    }
}
