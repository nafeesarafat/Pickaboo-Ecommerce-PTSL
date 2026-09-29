using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Data;
using Ecommerce.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminActivityLogService : IAdminActivityLogService
{
    private readonly AppDbContext _context;

    public AdminActivityLogService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<LoginLogDto>> GetLoginLogsAsync(LoginLogQueryDto query, CancellationToken cancellationToken)
    {
        var logs = _context.LoginLogs.AsNoTracking().AsQueryable();
        if (query.LogId.HasValue) logs = logs.Where(log => log.LogId == query.LogId);
        if (!string.IsNullOrWhiteSpace(query.AdministratorName)) logs = logs.Where(log => log.AdministratorName != null && log.AdministratorName.Contains(query.AdministratorName));
        if (query.LoginDateTime.HasValue) logs = logs.Where(log => log.LoginDateTime == query.LoginDateTime);
        if (!string.IsNullOrWhiteSpace(query.Status)) logs = logs.Where(log => log.Status == query.Status);

        return await logs.OrderByDescending(log => log.LoginDateTime)
            .Select(log => new LoginLogDto
            {
                LogId = log.LogId,
                AdministratorName = log.AdministratorName,
                LoginDateTime = log.LoginDateTime,
                Status = log.Status
            })
            .ToArrayAsync(cancellationToken);
    }

    public Task<IReadOnlyList<ActivityLogDto>> GetOrderActivityAsync(int orderId, ActivityLogQueryDto query, CancellationToken cancellationToken) =>
        GetActivityAsync(orderId, query, cancellationToken, "order");

    public Task<IReadOnlyList<ActivityLogDto>> GetProductActivityAsync(int productId, ActivityLogQueryDto query, CancellationToken cancellationToken) =>
        GetActivityAsync(productId, query, cancellationToken, "product");

    public Task<IReadOnlyList<ActivityLogDto>> GetUserActivityAsync(int userId, ActivityLogQueryDto query, CancellationToken cancellationToken) =>
        GetActivityAsync(userId, query, cancellationToken, "user");

    public async Task<IReadOnlyList<SystemLogDto>> GetSystemLogsAsync(SystemLogQueryDto query, CancellationToken cancellationToken)
    {
        var logs = _context.SystemLogs.AsNoTracking().AsQueryable();
        if (query.LogId.HasValue) logs = logs.Where(log => log.LogId == query.LogId);
        if (!string.IsNullOrWhiteSpace(query.Level)) logs = logs.Where(log => log.Level == query.Level);
        if (!string.IsNullOrWhiteSpace(query.Message)) logs = logs.Where(log => log.Message.Contains(query.Message));
        if (query.Timestamp.HasValue) logs = logs.Where(log => log.Timestamp == query.Timestamp);

        return await logs.OrderByDescending(log => log.Timestamp)
            .Select(log => new SystemLogDto
            {
                LogId = log.LogId,
                Level = log.Level,
                Message = log.Message,
                Timestamp = log.Timestamp
            })
            .ToArrayAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ActivityLogDto>> GetActivityAsync(
        int entityId,
        ActivityLogQueryDto query,
        CancellationToken cancellationToken,
        string entity)
    {
        if (entityId <= 0) throw new BadRequestException("A valid entity id is required.");
        var logs = _context.ActivityLogs.AsNoTracking().AsQueryable();
        logs = entity switch
        {
            "order" => logs.Where(log => log.OrderId == entityId),
            "product" => logs.Where(log => log.ProductId == entityId),
            "user" => logs.Where(log => log.UserId == entityId),
            _ => throw new InvalidOperationException("The activity-log entity type is unsupported.")
        };

        if (query.LogId.HasValue) logs = logs.Where(log => log.LogId == query.LogId);
        if (!string.IsNullOrWhiteSpace(query.Action)) logs = logs.Where(log => log.Action.Contains(query.Action));
        if (!string.IsNullOrWhiteSpace(query.Description)) logs = logs.Where(log => log.Description != null && log.Description.Contains(query.Description));
        if (query.Timestamp.HasValue) logs = logs.Where(log => log.Timestamp == query.Timestamp);

        return await logs.OrderByDescending(log => log.Timestamp)
            .Select(log => new ActivityLogDto
            {
                LogId = log.LogId,
                OrderId = log.OrderId,
                ProductId = log.ProductId,
                UserId = log.UserId,
                Action = log.Action,
                Description = log.Description,
                Timestamp = log.Timestamp
            })
            .ToArrayAsync(cancellationToken);
    }
}
