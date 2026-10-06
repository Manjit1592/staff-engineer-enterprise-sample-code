using Enterprise.NotificationService.Application.Contracts;

namespace Enterprise.NotificationService.Infrastructure.Providers;

public class SmtpEmailProvider : IEmailProvider
{
    public Task SendAsync(
        string to,
        string subject,
        string body)
    {
        Console.WriteLine(
            $"EMAIL => To:{to}, Subject:{subject}");

        return Task.CompletedTask;
    }
}