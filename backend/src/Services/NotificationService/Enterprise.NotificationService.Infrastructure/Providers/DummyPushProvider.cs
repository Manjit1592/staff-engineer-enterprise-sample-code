using Enterprise.NotificationService.Application.Contracts;

namespace Enterprise.NotificationService.Infrastructure.Providers;

public class DummyPushProvider : IPushProvider
{
    public Task SendAsync(
        string userId,
        string title,
        string message)
    {
        Console.WriteLine(
            $"PUSH => User:{userId}, Title:{title}");

        return Task.CompletedTask;
    }
}