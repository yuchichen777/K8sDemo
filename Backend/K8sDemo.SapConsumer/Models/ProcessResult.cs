namespace K8sDemo.SapConsumer.Models
{
    public class ProcessResult
    {
        public bool Success { get; set; }

        public bool ShouldRetry { get; set; }

        public bool SendToDlq { get; set; }
    }
}
