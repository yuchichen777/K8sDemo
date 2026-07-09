using K8sDemo.SapConsumer.HealthChecks;
using K8sDemo.SapConsumer.HostedServices;
using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Options;
using K8sDemo.SapConsumer.Services;
using K8sDemo.Shared.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<RetryOptions>(
    builder.Configuration.GetSection("Retry"));
builder.Services.Configure<SapApiOptions>(
    builder.Configuration.GetSection("SapApi"));

builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live", "ready"])
    .AddCheck<RabbitMqHealthCheck>(
        "rabbitmq",
        tags: ["ready"])
    .AddCheck<SapApiHealthCheck>(
        "sap-api",
        tags: ["ready"]);

builder.Services.AddHttpClient<ISapApiClient, SapApiClient>();

builder.Services.AddScoped<IDlqService, DlqService>();
builder.Services.AddScoped<IRetryService, RetryService>();
builder.Services.AddScoped<IMaterialEventProcessor, MaterialEventProcessor>();

builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

builder.Services.AddHostedService<SapConsumerService>();

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => "SapConsumer Running");
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/metrics", (IStatisticsService statisticsService) =>
{
    var statistics = statisticsService.GetStatistics();
    var dlqMessages = statisticsService.GetDlqMessages();

    return Results.Text(
        $"""
        # HELP k8sdemo_sap_consumer_success_total Total successfully processed SAP consumer events.
        # TYPE k8sdemo_sap_consumer_success_total counter
        k8sdemo_sap_consumer_success_total {statistics.SuccessCount}
        # HELP k8sdemo_sap_consumer_fail_total Total failed SAP consumer events.
        # TYPE k8sdemo_sap_consumer_fail_total counter
        k8sdemo_sap_consumer_fail_total {statistics.FailCount}
        # HELP k8sdemo_sap_consumer_retry_total Total SAP consumer retries.
        # TYPE k8sdemo_sap_consumer_retry_total counter
        k8sdemo_sap_consumer_retry_total {statistics.RetryCount}
        # HELP k8sdemo_sap_consumer_dlq_total Total SAP consumer events moved to DLQ.
        # TYPE k8sdemo_sap_consumer_dlq_total counter
        k8sdemo_sap_consumer_dlq_total {statistics.DlqCount}
        # HELP k8sdemo_sap_consumer_dlq_messages Current in-memory DLQ messages.
        # TYPE k8sdemo_sap_consumer_dlq_messages gauge
        k8sdemo_sap_consumer_dlq_messages {dlqMessages.Count}
        """,
        "text/plain; version=0.0.4");
});

app.Run();
