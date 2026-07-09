using K8sDemo.Shared.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace K8sDemo.SapConsumer.Services;

public class RabbitMqTopologyInitializer
{
    private readonly RabbitMqOptions _rabbitMqOptions;

    public RabbitMqTopologyInitializer(IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    public async Task InitializeAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _rabbitMqOptions.ExchangeName,
            type: ExchangeType.Direct);

        await channel.QueueDeclareAsync(
            queue: _rabbitMqOptions.MaterialDlqQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var retryArgs = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = _rabbitMqOptions.RetryDelayMilliseconds,
            ["x-dead-letter-exchange"] = _rabbitMqOptions.ExchangeName,
            ["x-dead-letter-routing-key"] = _rabbitMqOptions.MaterialRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: _rabbitMqOptions.MaterialRetryQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: retryArgs);

        var materialArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = "",
            ["x-dead-letter-routing-key"] = _rabbitMqOptions.MaterialDlqQueueName
        };

        await channel.QueueDeclareAsync(
            queue: _rabbitMqOptions.MaterialQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: materialArgs);

        await channel.QueueBindAsync(
            queue: _rabbitMqOptions.MaterialQueueName,
            exchange: _rabbitMqOptions.ExchangeName,
            routingKey: _rabbitMqOptions.MaterialRoutingKey);
    }
}
