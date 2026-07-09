using K8sDemo.SapApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<SapService>();
builder.Services.AddSingleton<SapMetricsService>();
builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live", "ready"]);

var app = builder.Build();

app.MapControllers();
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
app.MapGet("/metrics", (SapMetricsService metrics) =>
    Results.Text(
        $"""
        # HELP k8sdemo_sap_api_requests_total Total SAP API material-picked requests.
        # TYPE k8sdemo_sap_api_requests_total counter
        k8sdemo_sap_api_requests_total {metrics.RequestsTotal}
        # HELP k8sdemo_sap_api_success_total Total successful SAP API responses.
        # TYPE k8sdemo_sap_api_success_total counter
        k8sdemo_sap_api_success_total {metrics.SuccessTotal}
        # HELP k8sdemo_sap_api_failure_total Total failed SAP API responses.
        # TYPE k8sdemo_sap_api_failure_total counter
        k8sdemo_sap_api_failure_total {metrics.FailureTotal}
        """,
        "text/plain; version=0.0.4"));

app.Run();
