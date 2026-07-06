using System.Text;
using System.Text.Json;
using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;
using RabbitMQ.Client;

namespace K8sDemo.SapConsumer.Services;

public class DlqService : IDlqService
{
    private readonly IConfiguration _configuration;

    public DlqService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task RequeueAsync(string workOrder, string reelId)
    {
        var item =
            ConsumerStatistics.DlqMessages
                .FirstOrDefault(x =>
                    x.WorkOrder == workOrder &&
                    x.ReelId == reelId);

        if (item == null)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        await using var connection =
            await factory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "sap-events",
            type: ExchangeType.Direct
        );

        var evt =
            new MaterialPickedEvent
            {
                EventType = "MaterialPicked",
                WorkOrder = item.WorkOrder,
                ReelId = item.ReelId,
                Material = item.Material,
                Qty = item.Qty,
                Message = item.Message,
                RetryCount = 0,
                Time = DateTime.UtcNow
            };

        var body =
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(evt));

        await channel.BasicPublishAsync(
            exchange: "sap-events",
            routingKey: "material",
            body: body);

        ConsumerStatistics.DlqMessages.Remove(item);
    }
}
