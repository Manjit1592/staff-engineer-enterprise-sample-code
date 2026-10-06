using Enterprise.AnalyticsService.Domain.Entities;
using Enterprise.AnalyticsService.Persistence.Context;
using Enterprise.Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Enterprise.AnalyticsService.Infrastructure.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderCreatedConsumer(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672
};

        using var connection =
            factory.CreateConnection();

        using var channel =
            connection.CreateModel();

        channel.QueueDeclare(
            queue: "order-created-queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer =
            new EventingBasicConsumer(channel);

        consumer.Received += (sender, eventArgs) =>
        {
Console.WriteLine("MESSAGE RECEIVED FROM RABBITMQ");
            try
            {
                var body =
                    eventArgs.Body.ToArray();

                var json =
                    Encoding.UTF8.GetString(body);

Console.WriteLine(json);

                Console.WriteLine(
                    $"ANALYTICS EVENT RECEIVED: {json}");

                var orderEvent =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (orderEvent == null)
                {
                    return;
                }

                using var scope =
                    _scopeFactory.CreateScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<AnalyticsDbContext>();

                var today =
                    DateTime.UtcNow.Date;

                var metric =
                    dbContext.AnalyticsMetrics
                        .FirstOrDefault(x =>
                            x.MetricDate == today);

                if (metric == null)
                {
                    metric = new AnalyticsMetric
                    {
                        Id = Guid.NewGuid(),
                        MetricDate = today,
                        TotalOrders = 0,
                        TotalRevenue = 0
                    };

                    dbContext.AnalyticsMetrics
                        .Add(metric);
                }

                metric.TotalOrders++;

                metric.TotalRevenue +=
                    orderEvent.TotalAmount;

                metric.AverageOrderValue =
                    metric.TotalRevenue /
                    metric.TotalOrders;

                dbContext.SaveChanges();

                Console.WriteLine(
                    "ANALYTICS UPDATED");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"ANALYTICS ERROR: {ex.Message}");
            }
        };

        channel.BasicConsume(
            queue: "order-created-queue",
            autoAck: true,
            consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(
                5000,
                stoppingToken);
        }
    }
}