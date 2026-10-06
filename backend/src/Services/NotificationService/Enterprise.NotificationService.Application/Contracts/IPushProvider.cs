namespace Enterprise.NotificationService.Application.Contracts;

public interface IPushProvider
{
    Task SendAsync(
        string userId,
        string title,
        string message);
}