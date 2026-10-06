using Enterprise.NotificationService.Application.DTOs;

namespace Enterprise.NotificationService.Application.Services;

public interface INotificationService
{
    Task SendAsync(NotificationRequest request);
}
