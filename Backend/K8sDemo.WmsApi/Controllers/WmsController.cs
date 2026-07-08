using K8sDemo.Shared.Models;
using K8sDemo.Shared.Options;
using K8sDemo.WmsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace K8sDemo.WmsApi.Controllers;

[ApiController]
[Route("api/wms")]
public class WmsController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public WmsController(
        RabbitMqPublisher publisher,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _publisher = publisher;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    [HttpPost("material-picked")]
    public async Task<IActionResult> MaterialPicked()
    {
        var evt = new MaterialPickedEvent
        {
            EventType = "MaterialPicked",
            WorkOrder = "WO20260604001",
            Material = "R5100",
            ReelId = "REEL001",
            Qty = 100,
            Time = DateTime.Now
        };

        await _publisher.PublishAsync(
            _rabbitMqOptions.MaterialRoutingKey,
            evt
        );

        return Ok(new
        {
            Message = "MaterialPicked Published"
        });
    }

    [HttpPost("material-retry")]
    public async Task<IActionResult> MaterialRetry()
    {
        var evt = new MaterialPickedEvent
        {
            EventType = "MaterialPicked",
            WorkOrder = "WO20260604003",
            Material = "R5100",
            ReelId = "REEL_RETRY",
            Qty = 100,
            Message = "RETRY",
            RetryCount = 0,
            Time = DateTime.Now
        };

        await _publisher.PublishAsync(
            _rabbitMqOptions.MaterialRoutingKey,
            evt
        );

        return Ok(new
        {
            Message = "MaterialPicked RETRY Published"
        });
    }

    [HttpPost("material-fail")]
    public async Task<IActionResult> MaterialFail()
    {
        var evt = new MaterialPickedEvent
        {
            EventType = "MaterialPicked",
            WorkOrder = "WO20260604002",
            Material = "R5100",
            ReelId = "REEL_FAIL",
            Qty = 100,
            Message = "FAIL",
            Time = DateTime.Now
        };

        await _publisher.PublishAsync(
            _rabbitMqOptions.MaterialRoutingKey,
            evt
        );

        return Ok(new
        {
            Message = "MaterialPicked FAIL Published"
        });
    }
}
