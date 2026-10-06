using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Enterprise.EventBus.RabbitMQ.Publishers;

public class RabbitMqPublisher
{
    public void Publish<T>(string queueName, T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = "host.docker.internal"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
    }
}