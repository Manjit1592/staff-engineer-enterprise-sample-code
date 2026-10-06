using Confluent.Kafka;
using System.Text.Json;

namespace Enterprise.EventBus.Kafka.Producers;

public class KafkaProducer
{
    private readonly ProducerConfig _config;

public KafkaProducer(
    ProducerConfig config)
{
    _config = config;

    Console.WriteLine(
        $"Kafka Bootstrap Server: {_config.BootstrapServers}");
}

    public async Task PublishAsync<T>(
        string topicName,
        T message)
    {
        using var producer =
            new ProducerBuilder<Null, string>(_config)
                .Build();

        var json =
            JsonSerializer.Serialize(message);

        await producer.ProduceAsync(
            topicName,
            new Message<Null, string>
            {
                Value = json
            });

        producer.Flush(
    TimeSpan.FromSeconds(5));
    }
}