using Ecommerce.Api.DTOs.Admin;

namespace Ecommerce.Api.Services;

public interface IAdminNotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetAsync(NotificationQueryDto query, CancellationToken cancellationToken);
    Task<PromotionalNotificationResultDto> SendPromotionAsync(SendPromotionalNotificationRequestDto request, CancellationToken cancellationToken);
    Task<NotificationSettingsDto> UpdateSettingsAsync(NotificationSettingsDto request, CancellationToken cancellationToken);
}
