using K8sDemo.SapApi.Services;
using K8sDemo.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace K8sDemo.SapApi.Controllers;

[ApiController]
[Route("api/sap")]
public class SapController : ControllerBase
{
    private readonly SapService _sapService;

    public SapController(SapService sapService)
    {
        _sapService = sapService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Service = "SapApi",
            Status = "OK",
            Time = DateTime.Now
        });
    }

    [HttpPost("material-picked")]
    public async Task<IActionResult> MaterialPicked(MaterialPickedEvent evt)
    {
        var result = await _sapService.PostMaterialPickedAsync(evt);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}