using Microsoft.AspNetCore.Mvc;
using K8sDemo.Shared.Models;
using K8sDemo.WmsApi.Services;

namespace K8sDemo.WmsApi.Controllers;

[ApiController]
[Route("api/wms")]
public class WmsController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;

    public WmsController(
        RabbitMqPublisher publisher)
    {
        _publisher = publisher;
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
            "material",
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
            "material",
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
            "material",
            evt
        );

        return Ok(new
        {
            Message = "MaterialPicked FAIL Published"
        });
    }
}