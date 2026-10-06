using Enterprise.NotificationService.Application.Contracts;
using Enterprise.NotificationService.Domain.Enums;
using Enterprise.NotificationService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Enterprise.NotificationService.Infrastructure.BackgroundJobs;

public class NotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationWorker(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope =
                _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<NotificationDbContext>();

            var emailProvider =
                scope.ServiceProvider
                    .GetRequiredService<IEmailProvider>();

            var pendingNotifications =
    await dbContext.Notifications
        .Where(x =>
            x.Status == NotificationStatus.Pending
            ||
            (
                x.Status == NotificationStatus.Failed
                &&
                x.NextAttemptUtc != null
                &&
                x.NextAttemptUtc <= DateTime.UtcNow
            ))
        .OrderBy(x => x.CreatedOnUtc)
        .Take(10)
        .ToListAsync(stoppingToken);

            foreach (var notification in pendingNotifications)
            {
                try
                {
                    notification.Status =
                        NotificationStatus.Processing;

                    await dbContext.SaveChangesAsync(
                        stoppingToken);

                    if (notification.Type ==
    NotificationType.Email)
{
    await emailProvider.SendAsync(
        notification.Recipient,
        notification.Subject,
        notification.Message);
}

                    notification.Status =
                        NotificationStatus.Sent;

                    notification.SentOnUtc =
                        DateTime.UtcNow;

                    await dbContext.SaveChangesAsync(
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    notification.RetryCount++;

                    notification.FailureReason =
                        ex.Message;

                    if (notification.RetryCount >= 5)
                    {
                        notification.Status =
                            NotificationStatus.DeadLettered;

                        notification.NextAttemptUtc = null;
                    }
                    else
                    {
                        notification.Status =
                            NotificationStatus.Failed;

                        notification.NextAttemptUtc =
                            DateTime.UtcNow.AddMinutes(1);
                    }

                    await dbContext.SaveChangesAsync(
                        stoppingToken);
                }
            }

            Console.WriteLine(
                $"Processed {pendingNotifications.Count} notifications");

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }
}