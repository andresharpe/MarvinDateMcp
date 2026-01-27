using System.Text.Json;
using System.Text.Json.Serialization;
using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace MarvinDateMcp.Tests;

/// <summary>
/// Generates JSON results for all test locations for documentation purposes
/// </summary>
public class JsonResultsGenerator
{
    private readonly ServiceProvider _serviceProvider;
    
    public JsonResultsGenerator()
    {
        // Load .env.local file from project root
        var projectRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..");
        var envLocalPath = Path.Combine(projectRoot, ".env.local");
        
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

        // Build service collection
        var services = new ServiceCollection();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise
        });

        // Configure options
        services.Configure<GoogleApiOptions>(options =>
        {
            options.ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? string.Empty;
        });

        services.Configure<DateServiceOptions>(options =>
        {
            options.HolidayLookaheadDays = 365;
        });

        // Add HTTP clients with retry policies
        services.AddHttpClient<IGoogleGeocodingService, GoogleGeocodingService>()
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IHolidayService, HolidayService>()
            .AddPolicyHandler(GetRetryPolicy());

        services.AddSingleton<WeekendService>();
        services.AddScoped<IDateContextService, DateContextService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );
    }

    public async Task<string> GenerateJsonResultsAsync()
    {
        var locations = new[]
        {
            ("Houses of Parliament, Westminster, London", "GB"),
            ("White House, Washington DC", "US"),
            ("Eiffel Tower, Paris", "FR"),
            ("Tokyo Tower, Tokyo", "JP"),
            ("Sydney Opera House, Sydney", "AU"),
            ("Brandenburg Gate, Berlin", "DE"),
            ("Red Square, Moscow", "RU"),
            ("Christ the Redeemer, Rio de Janeiro", "BR"),
            ("CN Tower, Toronto", "CA"),
            ("Regus Central Station Brussels", "BE"),
            ("Bleicherweg 10, Zurich", "CH")
        };

        var results = new List<object>();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDateContextService>();

        foreach (var (location, countryCode) in locations)
        {
            try
            {
                Console.WriteLine($"Testing: {location}");
                var result = await service.AnalyzeDateContextAsync(location);
                
                results.Add(new
                {
                    location,
                    expectedCountryCode = countryCode,
                    success = true,
                    result
                });

                // Delay to avoid rate limiting
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    location,
                    expectedCountryCode = countryCode,
                    success = false,
                    error = ex.Message
                });
            }
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(results, options);
    }
}
