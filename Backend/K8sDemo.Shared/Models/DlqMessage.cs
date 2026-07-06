namespace K8sDemo.Shared.Models
{
    public class DlqMessage
    {
        public string WorkOrder { get; set; } = "";

        public string ReelId { get; set; } = "";

        public string Material { get; set; } = "";

        public int Qty { get; set; }

        public DateTime Time { get; set; }

        public int RetryCount { get; set; }

        public string ErrorMessage { get; set; } = "";

        public string? Message { get; set; }
    }
}
