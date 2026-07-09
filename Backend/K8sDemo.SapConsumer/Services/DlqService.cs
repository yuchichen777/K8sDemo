using System.Text;
using System.Text.Json;
using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;
using K8sDemo.Shared.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace K8sDemo.SapConsumer.Services;

public class DlqService : IDlqService
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly RabbitMqTopologyInitializer _topologyInitializer;
    private readonly IStatisticsService _statistics;

    public DlqService(
        IOptions<RabbitMqOptions> rabbitMqOptions,
        RabbitMqTopologyInitializer topologyInitializer,
        IStatisticsService statistics)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _topologyInitializer = topologyInitializer;
        _statistics = statistics;
    }

    public async Task RequeueAsync(string workOrder, string reelId)
    {
        var item =
            _statistics.GetDlqMessages()
                .FirstOrDefault(x =>
                    x.WorkOrder == workOrder &&
                    x.ReelId == reelId);

        if (item == null)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.Host,
            UserName = _rabbitMqOptions.Username,
            Password = _rabbitMqOptions.Password
        };

        await using var connection =
            await factory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        await _topologyInitializer.InitializeAsync(channel);

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
            exchange: _rabbitMqOptions.ExchangeName,
            routingKey: _rabbitMqOptions.MaterialRoutingKey,
            body: body);

        _statistics.RemoveDlqMessage(workOrder, reelId);
    }
}
