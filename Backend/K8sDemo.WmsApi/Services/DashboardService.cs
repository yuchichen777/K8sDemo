using K8sDemo.Shared.Models;
using K8sDemo.Shared.Options;
using K8sDemo.WmsApi.Options;
using Microsoft.Extensions.Options;

namespace K8sDemo.WmsApi.Services;

public class DashboardService
{
    private readonly HttpClient _httpClient;
    private readonly RabbitMqService _rabbitMqService;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly SapApiOptions _sapApiOptions;
    private readonly SapConsumerOptions _sapConsumerOptions;

    public DashboardService(
        HttpClient httpClient,
        RabbitMqService rabbitMqService,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IOptions<SapApiOptions> sapApiOptions,
        IOptions<SapConsumerOptions> sapConsumerOptions)
    {
        _httpClient = httpClient;
        _rabbitMqService = rabbitMqService;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _sapApiOptions = sapApiOptions.Value;
        _sapConsumerOptions = sapConsumerOptions.Value;
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
                $"{_sapApiOptions.BaseUrl}/api/sap/health"
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
                    $"{_sapConsumerOptions.BaseUrl}/api/statistics"
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
                    _rabbitMqOptions.MaterialQueueName
                );

            statistics.DlqQueueCount =
                await _rabbitMqService.GetQueueCountAsync(
                    _rabbitMqOptions.MaterialDlqQueueName
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
