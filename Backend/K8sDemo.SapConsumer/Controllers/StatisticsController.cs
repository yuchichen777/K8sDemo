using K8sDemo.Shared.Models;
using K8sDemo.SapConsumer.Services;
using Microsoft.AspNetCore.Mvc;

namespace K8sDemo.SapConsumer.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    [HttpGet]
    public ActionResult<DashboardStatistics> Get()
    {
        var processTimes = ConsumerStatistics.ProcessTimes.ToList();
        var eventTimes = ConsumerStatistics.EventTimes.ToList();

        var now = DateTime.UtcNow;

        var performance = new PerformanceStatistics
        {
            TotalEvents =
                ConsumerStatistics.SuccessCount +
                ConsumerStatistics.FailCount,

            AvgProcessMs =
                processTimes.Any()
                    ? processTimes.Average()
                    : 0,

            MaxProcessMs =
                processTimes.Any()
                    ? processTimes.Max()
                    : 0,

            MinProcessMs =
                processTimes.Any()
                    ? processTimes.Min()
                    : 0,

            EventsPerMinute =
                eventTimes.Count(x =>
                    x >= now.AddMinutes(-1))
        };

        return Ok(new DashboardStatistics
        {
            SuccessCount = ConsumerStatistics.SuccessCount,
            FailCount = ConsumerStatistics.FailCount,
            RetryCount = ConsumerStatistics.RetryCount,
            DlqCount = ConsumerStatistics.DlqCount,
            RecentEvents = ConsumerStatistics.RecentEvents.ToList(),
            TrendData = ConsumerStatistics.TrendData.ToList(),
            Performance = performance
        });
    }
}