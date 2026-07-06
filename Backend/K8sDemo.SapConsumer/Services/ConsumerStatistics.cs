using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Services;

public static class ConsumerStatistics
{
    public static int SuccessCount;

    public static int FailCount;

    public static int RetryCount;

    public static int DlqCount;

    public static List<EventLog> RecentEvents { get; } = [];

    public static List<StatisticsPoint> TrendData { get; } = [];

    public static List<DlqMessage> DlqMessages { get; } = [];

    public static List<double> ProcessTimes { get; } = [];

    public static List<DateTime> EventTimes { get; } = [];

    public static void AddLog(EventLog log)
    {
        lock (RecentEvents)
        {
            RecentEvents.Insert(0, log);

            if (RecentEvents.Count > 20)
            {
                RecentEvents.RemoveAt(
                    RecentEvents.Count - 1
                );
            }
        }
    }

    public static void AddTrend()
    {
        lock (TrendData)
        {
            TrendData.Add(
                new StatisticsPoint
                {
                    Time = DateTime.Now,
                    Success = SuccessCount,
                    Fail = FailCount,
                    Retry = RetryCount,
                    Dlq = DlqCount
                });

            if (TrendData.Count > 50)
            {
                TrendData.RemoveAt(0);
            }
        }
    }

    public static void AddProcessTime(double ms)
    {
        ProcessTimes.Add(ms);

        if (ProcessTimes.Count > 1000)
        {
            ProcessTimes.RemoveAt(0);
        }
    }

    public static void AddEventTime()
    {
        EventTimes.Add(DateTime.UtcNow);

        if (EventTimes.Count > 1000)
        {
            EventTimes.RemoveAt(0);
        }
    }
}