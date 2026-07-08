namespace K8sDemo.Shared.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = string.Empty;
    public int ManagementPort { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string MaterialQueueName { get; set; } = string.Empty;
    public string MaterialDlqQueueName { get; set; } = string.Empty;
    public string MaterialRoutingKey { get; set; } = string.Empty;
}
