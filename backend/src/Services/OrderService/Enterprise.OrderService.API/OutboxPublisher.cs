using Enterprise.EventBus.Kafka.Producers;
using Enterprise.OrderService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Enterprise.OrderService.Infrastructure.BackgroundJobs;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaProducer _kafkaProducer;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        KafkaProducer kafkaProducer,
        ILogger<OutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox Publisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<OrderDbContext>();

                var messages =
                    await dbContext.OutboxMessages
                        .Where(x => !x.IsProcessed)
                        .OrderBy(x => x.OccurredOnUtc)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await _kafkaProducer.PublishAsync(
                            "order-created",
                            message.Payload);

                        message.IsProcessed = true;
                        message.ProcessedOnUtc =
                            DateTime.UtcNow;
                        message.Error = null;

                        await dbContext.SaveChangesAsync(
                            stoppingToken);

                        _logger.LogInformation(
                            "Outbox message {MessageId} published successfully",
                            message.Id);
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message;

                        await dbContext.SaveChangesAsync(
                            stoppingToken);

                        _logger.LogError(
                            ex,
                            "Failed to publish outbox message {MessageId}",
                            message.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Outbox publisher failed");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }
}