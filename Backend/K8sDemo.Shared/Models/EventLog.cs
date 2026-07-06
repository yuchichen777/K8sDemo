namespace K8sDemo.Shared.Models;

public class EventLog
{
    public DateTime Time { get; set; }

    public string WorkOrder { get; set; } = "";

    public string ReelId { get; set; } = "";

    public string Result { get; set; } = "";
}