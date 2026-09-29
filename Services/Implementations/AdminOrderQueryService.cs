using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Repositories;

namespace Ecommerce.Api.Services;

public class AdminOrderQueryService : IAdminOrderQueryService
{
    private static readonly HashSet<string> SupportedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Confirmed",
        "Processing",
        "Shipped",
        "Delivered",
        "Returned",
        "Refunded"
    };

    private readonly IAdminOrderQueryRepository _repository;

    public AdminOrderQueryService(IAdminOrderQueryRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<AdminOrderViewDto>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken)
    {
        string normalizedStatus = status.Trim();
        if (!SupportedStatuses.Contains(normalizedStatus))
        {
            throw new BadRequestException(
                "Status must be one of: Confirmed, Processing, Shipped, Delivered, Returned, Refunded.");
        }

        string canonicalStatus = SupportedStatuses.First(
            supported => string.Equals(supported, normalizedStatus, StringComparison.OrdinalIgnoreCase));
        return _repository.GetByStatusAsync(canonicalStatus, cancellationToken);
    }

    public Task<PagedResultDto<AdminOrderViewDto>> SearchAsync(
        AdminOrderSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.FromDate.HasValue && request.ToDate.HasValue
            && request.FromDate.Value > request.ToDate.Value)
        {
            throw new BadRequestException("FromDate must be earlier than or equal to ToDate.");
        }

        return _repository.SearchAsync(request, cancellationToken);
    }
}
