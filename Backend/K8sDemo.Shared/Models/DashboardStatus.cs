namespace K8sDemo.Shared.Models;

public class DashboardStatus
{
    public string WmsApi { get; set; } = "";

    public string SapApi { get; set; } = "";

    public string RabbitMq { get; set; } = "";

    public string SapConsumer { get; set; } = "";

    public DateTime Time { get; set; }

    public DashboardStatistics Statistics { get; set; }
        = new();
}