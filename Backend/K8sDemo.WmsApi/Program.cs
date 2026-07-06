using K8sDemo.WmsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

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
app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy 111" }));

app.Run();
