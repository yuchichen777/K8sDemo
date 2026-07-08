using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Services;

public class StatisticsService : IStatisticsService
{
    private int _successCount;
    private int _failCount;
    private int _retryCount;
    private int _dlqCount;
    private readonly List<EventLog> _recentEvents = [];
    private readonly List<StatisticsPoint> _trendData = [];
    private readonly List<DlqMessage> _dlqMessages = [];
    private readonly List<double> _processTimes = [];
    private readonly List<DateTime> _eventTimes = [];

    public DashboardStatistics GetStatistics()
    {
        return new DashboardStatistics
        {
            SuccessCount = _successCount,
            FailCount = _failCount,
            RetryCount = _retryCount,
            DlqCount = _dlqCount,
            RecentEvents = _recentEvents.ToList(),
            TrendData = _trendData.ToList()
        };
    }

    public void AddLog(EventLog log)
    {
        _recentEvents.Insert(0, log);

        if (_recentEvents.Count > 20)
        {
            _recentEvents.RemoveAt(_recentEvents.Count - 1);
        }
    }

    public void AddTrend()
    {
        _trendData.Add(new StatisticsPoint
        {
            Time = DateTime.UtcNow,
            Success = _successCount,
            Fail = _failCount,
            Retry = _retryCount,
            Dlq = _dlqCount
        });

        if (_trendData.Count > 50)
        {
            _trendData.RemoveAt(0);
        }
    }

    public void AddProcessTime(double milliseconds)
    {
        _processTimes.Add(milliseconds);

        if (_processTimes.Count > 1000)
        {
            _processTimes.RemoveAt(0);
        }
    }

    public void AddEventTime()
    {
        _eventTimes.Add(DateTime.UtcNow);

        if (_eventTimes.Count > 1000)
        {
            _eventTimes.RemoveAt(0);
        }
    }

    public void AddDlqMessage(DlqMessage message) => _dlqMessages.Add(message);

    public List<DlqMessage> GetDlqMessages() => _dlqMessages;

    public bool RemoveDlqMessage(string workOrder, string reelId)
    {
        var item = _dlqMessages
            .FirstOrDefault(x =>
                x.WorkOrder == workOrder &&
                x.ReelId == reelId);

        if (item == null)
            return false;

        _dlqMessages.Remove(item);

        return true;
    }

    public void RecordSuccess(MaterialPickedEvent evt, double processMs)
    {
        _successCount++;

        AddProcessTime(processMs);
        AddEventTime();

        AddLog(new EventLog
        {
            Time = DateTime.UtcNow,
            WorkOrder = evt.WorkOrder,
            ReelId = evt.ReelId,
            Result = "Success"
        });

        AddTrend();
    }

    public void RecordRetry(MaterialPickedEvent evt, double processMs)
    {
        _retryCount++;

        AddProcessTime(processMs);
        AddEventTime();

        AddLog(new EventLog
        {
            Time = DateTime.UtcNow,
            WorkOrder = evt.WorkOrder,
            ReelId = evt.ReelId,
            Result = $"Retry {evt.RetryCount + 1}"
        });

        AddTrend();
    }

    public void RecordDlq(MaterialPickedEvent evt, string errorMessage, double processMs)
    {
        _failCount++;
        _dlqCount++;

        AddProcessTime(processMs);
        AddEventTime();

        AddLog(new EventLog
        {
            Time = DateTime.UtcNow,
            WorkOrder = evt.WorkOrder,
            ReelId = evt.ReelId,
            Result = "DLQ"
        });

        AddTrend();

        AddDlqMessage(new DlqMessage
        {
            WorkOrder = evt.WorkOrder,
            ReelId = evt.ReelId,
            Material = evt.Material,
            Qty = evt.Qty,
            RetryCount = evt.RetryCount,
            Time = DateTime.UtcNow,
            ErrorMessage = errorMessage
        });
    }
}