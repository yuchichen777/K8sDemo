using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace K8sDemo.WmsApi.Services;

public class RabbitMqPublisher
{
    private readonly IConfiguration _configuration;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishAsync(string routingKey, object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "sap-events",
            type: ExchangeType.Direct
        );

        var json = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "sap-events",
            routingKey: routingKey,
            body: body
        );

        Console.WriteLine(
            $"[RabbitMQ] Published {routingKey}"
        );
    }
}
