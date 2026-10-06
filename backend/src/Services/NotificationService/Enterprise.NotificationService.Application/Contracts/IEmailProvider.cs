namespace Enterprise.NotificationService.Application.Contracts;

public interface IEmailProvider
{
    Task SendAsync(
        string to,
        string subject,
        string body);
}