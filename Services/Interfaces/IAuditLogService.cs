using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAuditLogService
{
    Task<PagedResultDto<AuditLogDto>> GetAllAsync(AuditLogQueryDto query, CancellationToken cancellationToken);
    Task RecordAsync(int administratorId, string action, string? description, string status, CancellationToken cancellationToken);
}
