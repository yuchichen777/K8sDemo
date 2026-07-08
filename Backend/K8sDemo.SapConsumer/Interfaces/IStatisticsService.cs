using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Interfaces
{
    public interface IStatisticsService
    {
        DashboardStatistics GetStatistics();

        void AddLog(EventLog log);

        void AddTrend();

        void AddProcessTime(double milliseconds);

        void AddEventTime();

        void AddDlqMessage(DlqMessage message);

        bool RemoveDlqMessage(string workOrder, string reelId);

        List<DlqMessage> GetDlqMessages();

        void RecordSuccess(MaterialPickedEvent evt, double processMs);

        void RecordRetry(MaterialPickedEvent evt, double processMs);

        void RecordDlq(MaterialPickedEvent evt, string errorMessage, double processMs);
    }
}
