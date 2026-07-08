using K8sDemo.SapApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<SapService>();
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

app.Run();
