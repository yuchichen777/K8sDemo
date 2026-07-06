namespace K8sDemo.Shared.Models;

public class DashboardStatistics
{
    public int SuccessCount { get; set; }

    public int FailCount { get; set; }

    public int RetryCount { get; set; }

    public int DlqCount { get; set; }

    public int QueueCount { get; set; }

    public int DlqQueueCount { get; set; }

    public List<EventLog> RecentEvents { get; set; } = [];

    public List<StatisticsPoint> TrendData { get; set; } = [];

    public PerformanceStatistics Performance { get; set; } = new();
}