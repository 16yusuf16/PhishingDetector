using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PhishingDetector.API.Data;
using PhishingDetector.API.Services;
using PhishingDetector.Core.Interfaces;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<PhishingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.RemoveAllResilienceHandlers()
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    KeepAlivePingDelay = TimeSpan.FromSeconds(30),
    KeepAlivePingTimeout = TimeSpan.FromSeconds(15),
    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests,
    ConnectTimeout = TimeSpan.FromSeconds(15),
});
#pragma warning restore EXTEXP0001

builder.Services.AddScoped<IPhishingAnalysisService, PhishingAnalysisService>();

builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMinutes(15)
    };
});

builder.Services.AddCors(options =>
    options.AddPolicy("AllowBlazor", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PhishingDetector API",
        Version = "v1"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhishingDbContext>();
    db.Database.Migrate();
}

app.MapGet("/health", () => Results.Ok("Healthy"));
app.UseRequestTimeouts();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowBlazor");
app.UseAuthorization();
app.MapControllers();

app.Run();
