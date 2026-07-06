using K8sDemo.SapApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<SapService>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy" }));

app.Run();
