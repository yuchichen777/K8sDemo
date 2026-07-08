using K8sDemo.Shared.Options;
using K8sDemo.WmsApi.HealthChecks;
using K8sDemo.WmsApi.Options;
using K8sDemo.WmsApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<SapApiOptions>(
    builder.Configuration.GetSection("SapApi"));
builder.Services.Configure<SapConsumerOptions>(
    builder.Configuration.GetSection("SapConsumer"));

builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live", "ready"])
    .AddCheck<RabbitMqHealthCheck>(
        "rabbitmq",
        tags: ["ready"]);

builder.Services.AddHttpClient();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RabbitMqService>();
builder.Services.AddSingleton<RabbitMqPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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
