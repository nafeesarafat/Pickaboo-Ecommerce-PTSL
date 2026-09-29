using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminPaymentMethodService : IAdminPaymentMethodService
{
    private readonly AppDbContext _context;

    public AdminPaymentMethodService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminPaymentMethodDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.PaymentMethods
            .AsNoTracking()
            .OrderBy(method => method.PaymentMethodId)
            .Select(method => new AdminPaymentMethodDto
            {
                PaymentMethodId = method.PaymentMethodId,
                Name = method.Name,
                IsActive = method.IsActive
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AdminPaymentMethodDto> AddAsync(
        PaymentMethodRequestDto request,
        CancellationToken cancellationToken)
    {
        string name = NormalizeName(request.Name);
        await EnsureNameAvailableAsync(name, null, cancellationToken);

        var method = new PaymentMethod { Name = name, IsActive = request.IsActive };
        _context.PaymentMethods.Add(method);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(method);
    }

    public async Task<AdminPaymentMethodDto> UpdateAsync(
        int paymentMethodId,
        PaymentMethodRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateId(paymentMethodId);
        var method = await _context.PaymentMethods
            .SingleOrDefaultAsync(item => item.PaymentMethodId == paymentMethodId, cancellationToken)
            ?? throw new NotFoundException($"Payment method with id {paymentMethodId} was not found.");

        string name = NormalizeName(request.Name);
        await EnsureNameAvailableAsync(name, paymentMethodId, cancellationToken);
        method.Name = name;
        method.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(method);
    }

    public async Task DeleteAsync(int paymentMethodId, CancellationToken cancellationToken)
    {
        ValidateId(paymentMethodId);
        var method = await _context.PaymentMethods
            .SingleOrDefaultAsync(item => item.PaymentMethodId == paymentMethodId, cancellationToken)
            ?? throw new NotFoundException($"Payment method with id {paymentMethodId} was not found.");

        method.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameAvailableAsync(
        string name,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        if (await _context.PaymentMethods.AnyAsync(
                method => method.Name == name &&
                    (!excludedId.HasValue || method.PaymentMethodId != excludedId.Value),
                cancellationToken))
        {
            throw new ConflictException("A payment method with this name already exists.");
        }
    }

    private static string NormalizeName(string name)
    {
        string normalized = name.Trim();
        if (normalized.Length == 0)
        {
            throw new BadRequestException("A payment method name is required.");
        }

        return normalized;
    }

    private static void ValidateId(int paymentMethodId)
    {
        if (paymentMethodId <= 0)
        {
            throw new BadRequestException("A valid payment method id is required.");
        }
    }

    private static AdminPaymentMethodDto ToDto(PaymentMethod method)
    {
        return new AdminPaymentMethodDto
        {
            PaymentMethodId = method.PaymentMethodId,
            Name = method.Name,
            IsActive = method.IsActive
        };
    }
}
