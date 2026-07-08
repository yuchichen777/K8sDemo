using K8sDemo.Shared.Models;
using K8sDemo.WmsApi.Options;
using K8sDemo.WmsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace K8sDemo.WmsApi.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly DashboardService _dashboardService;
    private readonly SapConsumerOptions _sapConsumerOptions;

    public DashboardController(
        HttpClient httpClient,
        DashboardService dashboardService,
        IOptions<SapConsumerOptions> sapConsumerOptions)
    {
        _httpClient = httpClient;
        _dashboardService = dashboardService;
        _sapConsumerOptions = sapConsumerOptions.Value;
    }

    [HttpGet("status")]
    public async Task<ActionResult<DashboardStatus>> Status()
    {
        return Ok(
            await _dashboardService.GetStatusAsync()
        );
    }

    [HttpGet("dlq")]
    public async Task<IActionResult> GetDlq()
    {
        var result =
            await _httpClient.GetFromJsonAsync<List<DlqMessage>>(
                $"{_sapConsumerOptions.BaseUrl}/api/dlq"
            );

        return Ok(result);
    }

    [HttpPost("dlq/requeue")]
    public async Task<IActionResult> Requeue(RequeueRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                $"{_sapConsumerOptions.BaseUrl}/api/dlq/requeue",
                request);

        return Ok(
            await response.Content.ReadAsStringAsync());
    }
}
