using K8sDemo.Shared.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace K8sDemo.SapConsumer.HealthChecks;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqOptions _options;

    public RabbitMqHealthCheck(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                UserName = _options.Username,
                Password = _options.Password
            };

            await using var connection =
                await factory.CreateConnectionAsync(cancellationToken);

            return HealthCheckResult.Healthy("RabbitMQ is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ is unreachable.", ex);
        }
    }
}
