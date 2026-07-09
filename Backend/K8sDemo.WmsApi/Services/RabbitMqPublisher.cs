using K8sDemo.Shared.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace K8sDemo.WmsApi.Services;

public class RabbitMqPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly WmsMetricsService _metrics;

    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options,
        WmsMetricsService metrics)
    {
        _options = options.Value;
        _metrics = metrics;
    }

    public async Task PublishAsync(string routingKey, object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.Username,
            Password = _options.Password
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct
        );

        var json = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            body: body
        );

        Console.WriteLine(
            $"[RabbitMQ] Published {routingKey}"
        );

        _metrics.RecordPublished();
    }
}
