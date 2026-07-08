using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;
using K8sDemo.Shared.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace K8sDemo.SapConsumer.HostedServices;

public class SapConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public SapConsumerService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _scopeFactory = scopeFactory;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.Host,
            UserName = _rabbitMqOptions.Username,
            Password = _rabbitMqOptions.Password
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _rabbitMqOptions.ExchangeName,
            type: ExchangeType.Direct
        );

        await channel.QueueDeclareAsync(
            queue: _rabbitMqOptions.MaterialDlqQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

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
            arguments: materialArgs
        );

        await channel.QueueBindAsync(
            queue: _rabbitMqOptions.MaterialQueueName,
            exchange: _rabbitMqOptions.ExchangeName,
            routingKey: _rabbitMqOptions.MaterialRoutingKey
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<MaterialPickedEvent>(json);

            if (evt == null)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var processor =
                scope.ServiceProvider
                    .GetRequiredService<IMaterialEventProcessor>();

            var result = await processor.ProcessAsync(evt);

            if (result.Success)
            {
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            if (result.ShouldRetry)
            {
                await Task.Delay(3000, stoppingToken);

                await PublishRetryAsync(channel, evt);

                await channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
        };

        await channel.BasicConsumeAsync(
            queue: _rabbitMqOptions.MaterialQueueName,
            autoAck: false,
            consumer: consumer
        );

        Console.WriteLine("SapConsumer Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task PublishRetryAsync(IChannel channel, MaterialPickedEvent evt)
    {
        var json = JsonSerializer.Serialize(evt);

        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: _rabbitMqOptions.ExchangeName,
            routingKey: _rabbitMqOptions.MaterialRoutingKey,
            body: body
        );
    }
}
