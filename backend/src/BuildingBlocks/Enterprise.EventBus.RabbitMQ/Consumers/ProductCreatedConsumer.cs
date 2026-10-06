using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Enterprise.EventBus.RabbitMQ.Consumers;

public class ProductCreatedConsumer
{
    public void StartConsuming()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "host.docker.internal"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "product-created-queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);

            Console.WriteLine("======================================");
            Console.WriteLine("RABBITMQ MESSAGE CONSUMED:");
            Console.WriteLine(jsonMessage);
            Console.WriteLine("======================================");
        };

        channel.BasicConsume(queue: "product-created-queue",
                             autoAck: true,
                             consumer: consumer);
    }
}