using Ecommerce.Api.Data;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    public AuditLogService(AppDbContext context) => _context = context;

    public async Task<PagedResultDto<AuditLogDto>> GetAllAsync(AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        var items = _context.AuditLogs.AsNoTracking().AsQueryable();
        if (query.AuditLogId.HasValue) items = items.Where(x => x.AuditLogId == query.AuditLogId);
        if (query.AdministratorId.HasValue) items = items.Where(x => x.AdministratorId == query.AdministratorId);
        if (!string.IsNullOrWhiteSpace(query.Keyword)) items = items.Where(x => x.Action.Contains(query.Keyword) || (x.Description != null && x.Description.Contains(query.Keyword)) || x.Administrator.FullName.Contains(query.Keyword));
        if (!string.IsNullOrWhiteSpace(query.Status)) items = items.Where(x => x.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.AdministratorName)) items = items.Where(x => x.Administrator.FullName.Contains(query.AdministratorName));
        if (!string.IsNullOrWhiteSpace(query.Action)) items = items.Where(x => x.Action.Contains(query.Action));
        if (!string.IsNullOrWhiteSpace(query.Description)) items = items.Where(x => x.Description != null && x.Description.Contains(query.Description));
        if (query.FromDate.HasValue) items = items.Where(x => x.Timestamp >= query.FromDate);
        if (query.ToDate.HasValue) items = items.Where(x => x.Timestamp <= query.ToDate);
        if (query.Timestamp.HasValue) items = items.Where(x => x.Timestamp == query.Timestamp);
        var total = await items.CountAsync(cancellationToken);
        var data = await items.OrderByDescending(x => x.Timestamp).Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new AuditLogDto { AuditLogId = x.AuditLogId, AdministratorId = x.AdministratorId, AdministratorName = x.Administrator.FullName, Action = x.Action, Description = x.Description, Timestamp = x.Timestamp, Status = x.Status })
            .ToArrayAsync(cancellationToken);
        return new PagedResultDto<AuditLogDto> { Items = data, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)query.PageSize), PageNumber = query.PageNumber, PageSize = query.PageSize };
    }

    public async Task RecordAsync(int administratorId, string action, string? description, string status, CancellationToken cancellationToken)
    {
        if (!await _context.Administrators.AnyAsync(x => x.AdministratorId == administratorId, cancellationToken))
            return;
        _context.AuditLogs.Add(new AuditLog { AdministratorId = administratorId, Action = action, Description = description, Status = status, Timestamp = DateTime.UtcNow });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
