using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Data;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly AppDbContext _context;

    public AdminNotificationService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<NotificationDto>> GetAsync(NotificationQueryDto query, CancellationToken cancellationToken)
    {
        var notifications = _context.Notifications.AsNoTracking().AsQueryable();
        if (query.NotificationId.HasValue) notifications = notifications.Where(item => item.NotificationId == query.NotificationId);
        if (!string.IsNullOrWhiteSpace(query.Type)) notifications = notifications.Where(item => item.Type == query.Type);
        if (!string.IsNullOrWhiteSpace(query.Message)) notifications = notifications.Where(item => item.Message.Contains(query.Message));
        if (query.IsRead.HasValue) notifications = notifications.Where(item => item.IsRead == query.IsRead);
        if (query.CreatedDate.HasValue) notifications = notifications.Where(item => item.CreatedDate == query.CreatedDate);
        if (query.OrderId.HasValue) notifications = notifications.Where(item => item.OrderId == query.OrderId);
        if (query.UserId.HasValue) notifications = notifications.Where(item => item.UserId == query.UserId);
        if (query.ProductId.HasValue) notifications = notifications.Where(item => item.ProductId == query.ProductId);
        if (query.HasOrder == true) notifications = notifications.Where(item => item.OrderId != null);
        if (query.HasUser == true) notifications = notifications.Where(item => item.UserId != null);
        if (query.HasProduct == true) notifications = notifications.Where(item => item.ProductId != null);

        return await notifications.OrderByDescending(item => item.CreatedDate)
            .Select(item => new NotificationDto
            {
                NotificationId = item.NotificationId,
                Type = item.Type,
                Message = item.Message,
                IsRead = item.IsRead,
                CreatedDate = item.CreatedDate,
                OrderId = item.OrderId,
                UserId = item.UserId,
                ProductId = item.ProductId
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PromotionalNotificationResultDto> SendPromotionAsync(
        SendPromotionalNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        string audience = request.TargetAudience.Trim();
        if (!audience.Equals("All", StringComparison.OrdinalIgnoreCase)
            && !audience.Equals("AllCustomers", StringComparison.OrdinalIgnoreCase)
            && !audience.Equals("ActiveCustomers", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("TargetAudience must be All, AllCustomers, or ActiveCustomers.");
        }

        bool includeInactive = audience.Equals("All", StringComparison.OrdinalIgnoreCase)
            || audience.Equals("AllCustomers", StringComparison.OrdinalIgnoreCase);
        var recipients = _context.UserInformation
            .Where(user => user.Role == "Customer" && (includeInactive || user.IsActive));
        int[] userIds = await recipients.Select(user => user.UserId).ToArrayAsync(cancellationToken);
        int[] optedOutIds = await _context.NotificationSettings.AsNoTracking()
            .Where(settings => settings.UserId.HasValue
                && userIds.Contains(settings.UserId.Value)
                && !settings.PromotionalAlerts)
            .Select(settings => settings.UserId!.Value)
            .ToArrayAsync(cancellationToken);
        var optedOutUserIds = optedOutIds.ToHashSet();

        DateTime now = DateTime.UtcNow;
        string message = $"{request.Title.Trim()}: {request.Message.Trim()}";
        var notifications = userIds
            .Where(userId => !optedOutUserIds.Contains(userId))
            .Select(userId => new Notification
            {
                Type = "Promotional",
                Message = message,
                UserId = userId,
                IsRead = false,
                CreatedDate = now
            })
            .ToArray();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);
        return new PromotionalNotificationResultDto { NotificationsCreated = notifications.Length };
    }

    public async Task<NotificationSettingsDto> UpdateSettingsAsync(
        NotificationSettingsDto request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.NotificationSettings
            .Where(item => item.UserId == null)
            .OrderBy(item => item.SettingsId)
            .FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            settings = new NotificationSettings { UserId = null };
            _context.NotificationSettings.Add(settings);
        }

        settings.OrderAlerts = request.OrderAlerts;
        settings.StockAlerts = request.StockAlerts;
        settings.PromotionalAlerts = request.PromotionalAlerts;
        await _context.SaveChangesAsync(cancellationToken);
        return new NotificationSettingsDto
        {
            OrderAlerts = settings.OrderAlerts,
            StockAlerts = settings.StockAlerts,
            PromotionalAlerts = settings.PromotionalAlerts
        };
    }
}
