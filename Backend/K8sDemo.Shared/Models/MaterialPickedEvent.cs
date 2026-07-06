namespace K8sDemo.Shared.Models;

public class MaterialPickedEvent
{
    public string EventType { get; set; } = "";

    public string WorkOrder { get; set; } = "";

    public string Material { get; set; } = "";

    public string ReelId { get; set; } = "";

    public int Qty { get; set; }

    public string? Message { get; set; }

    public DateTime Time { get; set; }

    public int RetryCount { get; set; }
}
