namespace K8sDemo.Shared.Models;

public class PerformanceStatistics
{
    public int TotalEvents { get; set; }

    public double AvgProcessMs { get; set; }

    public double MaxProcessMs { get; set; }

    public double MinProcessMs { get; set; }

    public double EventsPerMinute { get; set; }
}