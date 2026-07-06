namespace K8sDemo.Shared.Models
{
    public class StatisticsPoint
    {
        public DateTime Time { get; set; }

        public string EventType { get; set; } = "";

        public int Success { get; set; }

        public int Fail { get; set; }

        public int Retry { get; set; }

        public int Dlq { get; set; }
    }
}
