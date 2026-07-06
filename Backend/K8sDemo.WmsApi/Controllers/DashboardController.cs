using K8sDemo.Shared.Models;
using K8sDemo.WmsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace K8sDemo.WmsApi.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly DashboardService _dashboardService;

    public DashboardController(
        HttpClient httpClient,
        DashboardService dashboardService)
    {
        _httpClient = httpClient;
        _dashboardService = dashboardService;
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
                "http://sap-consumer:8080/api/dlq"
            );

        return Ok(result);
    }

    [HttpPost("dlq/requeue")]
    public async Task<IActionResult> Requeue(RequeueRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "http://sap-consumer:8080/api/dlq/requeue",
                request);

        return Ok(
            await response.Content.ReadAsStringAsync());
    }
}