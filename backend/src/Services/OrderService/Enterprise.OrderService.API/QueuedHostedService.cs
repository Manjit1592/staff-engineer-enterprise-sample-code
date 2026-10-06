using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Enterprise.OrderService.Infrastructure.BackgroundJobs;

namespace Enterprise.OrderService.API;

public class QueuedHostedService : BackgroundService
{
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        BackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Background worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem =
                await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Background task failed");
            }
        }
    }
}