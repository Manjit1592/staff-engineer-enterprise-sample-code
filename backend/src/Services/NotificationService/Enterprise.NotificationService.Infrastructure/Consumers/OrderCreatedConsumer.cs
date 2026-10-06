using Enterprise.Common.Events;
using Enterprise.NotificationService.Domain.Entities;
using Enterprise.NotificationService.Domain.Enums;
using Enterprise.NotificationService.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Enterprise.NotificationService.Infrastructure.Consumers;

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
        Console.WriteLine("NOTIFICATION CONSUMER STARTED");

        var factory = new ConnectionFactory
        {
            HostName = "enterprise_rabbitmq",
            Port = 5672
        };

        using var connection =
            factory.CreateConnection();

        Console.WriteLine("RABBITMQ CONNECTED");

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
            try
            {
                var body =
                    eventArgs.Body.ToArray();

                var json =
                    Encoding.UTF8.GetString(body);

                Console.WriteLine(
                    $"ORDER EVENT RECEIVED: {json}");

                var options =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                var orderEvent =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(
                        json,
                        options);

                if (orderEvent == null)
                {
                    return;
                }

                using var scope =
                    _scopeFactory.CreateScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<NotificationDbContext>();

                var notification =
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        Type = NotificationType.Email,
                        Recipient = orderEvent.CustomerEmail,
                        Subject = "Order Created",
                        Message =
                            $"Your order {orderEvent.OrderId} was created successfully.",
                        Status = NotificationStatus.Pending,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                dbContext.Notifications.Add(
                    notification);

                dbContext.SaveChanges();

                Console.WriteLine(
                    "NOTIFICATION CREATED");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"CONSUMER ERROR: {ex}");
            }
        };

        Console.WriteLine("WAITING FOR EVENTS");

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