using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class StoreSettingsService : IStoreSettingsService
{
    private readonly AppDbContext _context;

    public StoreSettingsService(AppDbContext context) => _context = context;

    public async Task<GeneralStoreSettingsDto> GetGeneralAsync(CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        return new GeneralStoreSettingsDto
        {
            StoreName = settings.StoreName,
            LogoUrl = settings.LogoUrl,
            Timezone = settings.Timezone
        };
    }

    public async Task UpdateStoreAsync(UpdateStoreInformationRequestDto request, CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        settings.StoreName = request.StoreName.Trim();
        settings.Address = NullIfWhiteSpace(request.Address);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateContactAsync(UpdateContactInformationRequestDto request, CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        settings.Phone = NullIfWhiteSpace(request.Phone);
        settings.Email = NullIfWhiteSpace(request.Email);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCurrencyAsync(UpdateCurrencySettingsRequestDto request, CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        settings.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        settings.Symbol = request.Symbol.Trim();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateShippingAsync(UpdateShippingSettingsRequestDto request, CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        settings.DefaultShippingFee = request.DefaultShippingFee;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateEmailAsync(UpdateEmailSettingsRequestDto request, CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        settings.SmtpHost = request.SmtpHost.Trim();
        settings.SmtpPort = request.SmtpPort;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<StoreSettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _context.StoreSettings.SingleOrDefaultAsync(item => item.StoreKey == "default", cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new StoreSettings { StoreKey = "default", StoreName = "Store" };
        _context.StoreSettings.Add(settings);
        await _context.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
