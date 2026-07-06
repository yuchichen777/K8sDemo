using K8sDemo.Shared.Models;

namespace K8sDemo.WmsApi.Services;

public class DashboardService
{
    private readonly HttpClient _httpClient;
    private readonly RabbitMqService _rabbitMqService;

    public DashboardService(HttpClient httpClient, RabbitMqService rabbitMqService)
    {
        _httpClient = httpClient;
        _rabbitMqService = rabbitMqService;
    }

    public async Task<DashboardStatus> GetStatusAsync()
    {
        var sapStatus = "DOWN";
        var sapConsumerStatus = "DOWN";
        var rabbitMqStatus = "DOWN";

        var statistics = new DashboardStatistics();

        try
        {
            var response = await _httpClient.GetAsync(
                "http://sap-api:8080/api/sap/health"
            );

            if (response.IsSuccessStatusCode)
            {
                sapStatus = "OK";
            }
        }
        catch
        {
            sapStatus = "DOWN";
        }

        try
        {
            statistics =
                await _httpClient.GetFromJsonAsync<DashboardStatistics>(
                    "http://sap-consumer:8080/api/statistics"
                )
                ?? new DashboardStatistics();

            sapConsumerStatus = "Running";
        }
        catch
        {
            sapConsumerStatus = "DOWN";
        }

        try
        {
            statistics.QueueCount =
                await _rabbitMqService.GetQueueCountAsync(
                    "sap-material"
                );

            statistics.DlqQueueCount =
                await _rabbitMqService.GetQueueCountAsync(
                    "sap-material-dlq"
                );

            rabbitMqStatus =
                statistics.QueueCount >= 0 &&
                statistics.DlqQueueCount >= 0
                    ? "OK"
                    : "DOWN";
        }
        catch
        {
            rabbitMqStatus = "DOWN";
        }

        return new DashboardStatus
        {
            WmsApi = "OK",
            SapApi = sapStatus,
            RabbitMq = rabbitMqStatus,
            SapConsumer = sapConsumerStatus,
            Time = DateTime.Now,
            Statistics = statistics
        };
    }
}