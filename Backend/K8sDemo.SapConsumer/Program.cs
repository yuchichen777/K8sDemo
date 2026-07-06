using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient<ISapApiClient, SapApiClient>();

builder.Services.AddScoped<IDlqService, DlqService>();
builder.Services.AddScoped<IRetryService, RetryService>();
builder.Services.AddScoped<IMaterialEventProcessor, MaterialEventProcessor>();

builder.Services.AddHostedService<SapConsumerService>();

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => "SapConsumer Running");
app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy" }));

app.Run();
