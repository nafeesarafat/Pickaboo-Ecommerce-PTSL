using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminPaymentMethodService
{
    Task<IReadOnlyList<AdminPaymentMethodDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AdminPaymentMethodDto> AddAsync(
        PaymentMethodRequestDto request,
        CancellationToken cancellationToken);
    Task<AdminPaymentMethodDto> UpdateAsync(
        int paymentMethodId,
        PaymentMethodRequestDto request,
        CancellationToken cancellationToken);
    Task DeleteAsync(int paymentMethodId, CancellationToken cancellationToken);
}
