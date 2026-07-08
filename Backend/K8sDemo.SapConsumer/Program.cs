using K8sDemo.SapConsumer.HostedServices;
using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Services;

using K8sDemo.SapConsumer.Options;

using K8sDemo.Shared.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<RetryOptions>(
    builder.Configuration.GetSection("Retry"));
builder.Services.Configure<SapApiOptions>(
    builder.Configuration.GetSection("SapApi"));

builder.Services.AddHttpClient<ISapApiClient, SapApiClient>();

builder.Services.AddScoped<IDlqService, DlqService>();
builder.Services.AddScoped<IRetryService, RetryService>();
builder.Services.AddScoped<IMaterialEventProcessor, MaterialEventProcessor>();

builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

builder.Services.AddHostedService<SapConsumerService>();

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => "SapConsumer Running");
app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy" }));

app.Run();
