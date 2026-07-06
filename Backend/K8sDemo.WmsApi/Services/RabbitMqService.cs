using System.Net.Http.Headers;
using System.Text;

namespace K8sDemo.WmsApi.Services;

public class RabbitMqService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RabbitMqService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        var username =
            _configuration["RabbitMQ:Username"] ?? "guest";

        var password =
            _configuration["RabbitMQ:Password"] ?? "guest";

        var auth =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                auth
            );
    }

    public async Task<int> GetQueueCountAsync(
        string queueName)
    {
        var host =
            _configuration["RabbitMQ:Host"] ?? "rabbitmq";

        var managementPort =
            _configuration["RabbitMQ:ManagementPort"] ?? "15672";

        try
        {
            var response =
                await _httpClient.GetFromJsonAsync<RabbitQueue>(
                    $"http://{host}:{managementPort}/api/queues/%2F/{queueName}"
                );

            return response?.messages ?? 0;
        }
        catch
        {
            return -1;
        }
    }
}

public class RabbitQueue
{
    public int messages { get; set; }
}
