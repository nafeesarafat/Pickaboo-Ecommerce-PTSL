using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminActivityLogService
{
    Task<IReadOnlyList<LoginLogDto>> GetLoginLogsAsync(LoginLogQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<ActivityLogDto>> GetOrderActivityAsync(int orderId, ActivityLogQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<ActivityLogDto>> GetProductActivityAsync(int productId, ActivityLogQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<ActivityLogDto>> GetUserActivityAsync(int userId, ActivityLogQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyList<SystemLogDto>> GetSystemLogsAsync(SystemLogQueryDto query, CancellationToken cancellationToken);
}
