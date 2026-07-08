using K8sDemo.WmsApi.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace K8sDemo.WmsApi.Services;

public class RabbitMqService
{
    private readonly HttpClient _httpClient;
    private readonly RabbitMqOptions _options;

    public RabbitMqService(
        HttpClient httpClient,
        IOptions<RabbitMqOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        var auth =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                auth
            );
    }

    public async Task<int> GetQueueCountAsync(
        string queueName)
    {
        try
        {
            var response =
                await _httpClient.GetFromJsonAsync<RabbitQueue>(
                    $"http://{_options.Host}:{_options.ManagementPort}/api/queues/%2F/{queueName}"
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
