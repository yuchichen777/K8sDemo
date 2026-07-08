using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace K8sDemo.SapConsumer.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statistics;

    public StatisticsController(IStatisticsService statistics)
    {
        _statistics = statistics;
    }

    [HttpGet]
    public ActionResult<DashboardStatistics> Get()
    {
        return Ok(_statistics.GetStatistics());
    }
}