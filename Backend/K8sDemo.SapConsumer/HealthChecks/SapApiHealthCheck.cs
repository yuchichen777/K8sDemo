using K8sDemo.Shared.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace K8sDemo.SapConsumer.HealthChecks;

public class SapApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SapApiOptions _options;

    public SapApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<SapApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response =
                await httpClient.GetAsync(
                    $"{_options.BaseUrl}/api/sap/health",
                    cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("SAP API is reachable.");
            }

            return HealthCheckResult.Unhealthy(
                $"SAP API returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SAP API is unreachable.", ex);
        }
    }
}
