using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Security;
using MarvinDateMcp.Api.Services;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using System.Threading.RateLimiting;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

Console.WriteLine("=== MarvinDateMcp Starting ===");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"URLs: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");

try
{

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

// Azure Key Vault integration (only in Azure, not local dev)
var keyVaultUri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
if (!string.IsNullOrEmpty(keyVaultUri))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
        Log.Information("Azure Key Vault configured: {KeyVaultUri}", keyVaultUri);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to configure Azure Key Vault. Using App Settings fallback.");
    }
}

// Authentication - API Key
var apiKey = Environment.GetEnvironmentVariable("MCP_API_KEY")
    ?? builder.Configuration["Security:ApiKey"];

if (!string.IsNullOrEmpty(apiKey))
{
    builder.Services.AddAuthentication("ApiKey")
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options =>
        {
            options.ApiKey = apiKey;
        });

    builder.Services.AddAuthorization();
    Log.Information("API Key authentication configured");
}
else
{
    Log.Warning("MCP_API_KEY not configured - authentication disabled (NOT SUITABLE FOR PRODUCTION)");
}

// Rate Limiting - configurable via appsettings or environment
var rateLimitOptions = builder.Configuration
    .GetSection(RateLimitOptions.SectionName)
    .Get<RateLimitOptions>() ?? new RateLimitOptions();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = rateLimitOptions.PermitLimit,
                QueueLimit = rateLimitOptions.QueueLimit,
                Window = TimeSpan.FromMinutes(rateLimitOptions.WindowMinutes)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Rate limit exceeded. Please try again later.", token);

        Log.Warning("Rate limit exceeded for {RemoteIp}",
            context.HttpContext.Connection.RemoteIpAddress);
    };
});

Log.Information("Rate limiting configured: {PermitLimit} requests per {WindowMinutes} minute(s) per IP",
    rateLimitOptions.PermitLimit, rateLimitOptions.WindowMinutes);

// CORS Configuration
var allowedOrigins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://localhost:5001" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Mcp-Session-Id");
    });
});

Log.Information("CORS configured for origins: {Origins}", string.Join(", ", allowedOrigins));

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

// Retry policy - configurable via appsettings
var retryOptions = builder.Configuration
    .GetSection(RetryPolicyOptions.SectionName)
    .Get<RetryPolicyOptions>() ?? new RetryPolicyOptions();

// Add services with HttpClient and retry policies
builder.Services.AddHttpClient<IGoogleGeocodingService, GoogleGeocodingService>()
    .AddPolicyHandler(GetRetryPolicy(retryOptions))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(retryOptions.TimeoutSeconds));

builder.Services.AddHttpClient<IHolidayService, HolidayService>()
    .AddPolicyHandler(GetRetryPolicy(retryOptions))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(retryOptions.TimeoutSeconds));

Log.Information("Retry policy configured: {RetryCount} retries with {BaseDelay}s base delay, {Timeout}s timeout per attempt",
    retryOptions.RetryCount, retryOptions.BaseDelaySeconds, retryOptions.TimeoutSeconds);

builder.Services.AddSingleton<WeekendService>();
builder.Services.AddScoped<IDateContextService, DateContextService>();

builder.Services.AddHealthChecks();

// Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();
Log.Information("Application Insights telemetry configured");

var app = builder.Build();

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});

// Enable rate limiting
app.UseRateLimiter();

// Enable CORS
app.UseCors("SecureCorsPolicy");

// Enable authentication and authorization
if (!string.IsNullOrEmpty(apiKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Map MCP endpoint with authentication
var mcpEndpoint = app.MapMcp("/mcp");
if (!string.IsNullOrEmpty(apiKey))
{
    mcpEndpoint.RequireAuthorization();
    Log.Information("MCP endpoint protected with authentication");
}
else
{
    Log.Warning("MCP endpoint NOT protected - authentication disabled");
}

// Health check endpoint
app.MapHealthChecks("/health");

// Simple homepage
app.MapGet("/", () => "MarvinDateMcp - Date Context MCP Server");

// Log startup
Console.WriteLine("=== MarvinDateMcp Ready, calling app.Run() ===");
app.Logger.LogInformation("MarvinDateMcp starting up...");

app.Run();

}
catch (Exception ex)
{
    Console.WriteLine($"FATAL: {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}

// Retry policy with progressive delays (configurable)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(RetryPolicyOptions options)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: options.RetryCount,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(options.BaseDelaySeconds, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Retry {RetryCount} after {Delay}s due to {Reason}",
                    retryCount, timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
            });
}
