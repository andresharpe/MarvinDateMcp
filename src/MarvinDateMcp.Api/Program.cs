using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Services;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load .env.local if it exists
var envLocalPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".env.local");
if (File.Exists(envLocalPath))
{
    foreach (var line in File.ReadAllLines(envLocalPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// MCP Server Services
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Configure options
builder.Services.Configure<GoogleApiOptions>(options =>
{
    var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
        ?? builder.Configuration["Google:ApiKey"];
    options.ApiKey = apiKey ?? string.Empty;
});

builder.Services.Configure<DateServiceOptions>(
    builder.Configuration.GetSection(DateServiceOptions.SectionName));

// Add services with HttpClient and retry policies
builder.Services.AddHttpClient<IGoogleGeocodingService, GoogleGeocodingService>()
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient<IHolidayService, HolidayService>()
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddSingleton<WeekendService>();
builder.Services.AddScoped<IDateContextService, DateContextService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Map MCP endpoint
app.MapMcp("/mcp");

// Health check endpoint
app.MapHealthChecks("/health");

// Simple homepage
app.MapGet("/", () => "MarvinDateMcp - Date Context MCP Server");

// Log startup
app.Logger.LogInformation("MarvinDateMcp starting up...");

app.Run();

// Retry policy with progressive delays
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Retry {RetryCount} after {Delay}s due to {Reason}",
                    retryCount, timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
            });
}
