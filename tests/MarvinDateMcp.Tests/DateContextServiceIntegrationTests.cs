using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Xunit;
using Xunit.Abstractions;

namespace MarvinDateMcp.Tests;

/// <summary>
/// Integration tests for DateContextService with real Google API calls
/// Tests 10 well-known locations around the world
/// </summary>
public class DateContextServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ITestOutputHelper _output;

    public DateContextServiceIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

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
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configure options
        services.Configure<GoogleApiOptions>(options =>
        {
            options.ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? string.Empty;
        });

        services.Configure<DateServiceOptions>(options =>
        {
            options.HolidayLookaheadDays = 90; // Default value
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

    /// <summary>
    /// Test data: 11 well-known locations around the world
    /// </summary>
    public static TheoryData<string, string> TestLocations => new()
    {
        { "Houses of Parliament, Westminster, London", "GB" },
        { "White House, Washington DC", "US" },
        { "Eiffel Tower, Paris", "FR" },
        { "Tokyo Tower, Tokyo", "JP" },
        { "Sydney Opera House, Sydney", "AU" },
        { "Brandenburg Gate, Berlin", "DE" },
        { "Red Square, Moscow", "RU" },
        { "Christ the Redeemer, Rio de Janeiro", "BR" },
        { "CN Tower, Toronto", "CA" },
        { "Regus Central Station Brussels", "BE" },
        { "Bleicherweg 10, Zurich", "CH" }
    };

    [Theory]
    [MemberData(nameof(TestLocations))]
    public async Task AnalyzeDateContextAsync_ShouldReturnValidContext_ForWellKnownLocations(
        string location,
        string expectedCountryCode)
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDateContextService>();

        _output.WriteLine($"\n=== Testing Location: {location} ===");

        // Act
        var result = await service.AnalyzeDateContextAsync(location);

        // Assert - Location Information
        Assert.NotNull(result);
        Assert.NotNull(result.Location);
        Assert.NotEmpty(result.Location.ResolvedName);
        Assert.NotEmpty(result.Location.FormattedAddress);
        Assert.Equal(expectedCountryCode, result.Location.CountryCode);
        Assert.NotEmpty(result.Location.Timezone);
        Assert.NotEmpty(result.Location.UtcOffset);
        Assert.NotEmpty(result.Location.CurrentLocalTime);

        _output.WriteLine($"Resolved Name: {result.Location.ResolvedName}");
        _output.WriteLine($"Country Code: {result.Location.CountryCode}");
        _output.WriteLine($"Timezone: {result.Location.Timezone}");
        _output.WriteLine($"Current Local Time: {result.Location.CurrentLocalTime}");

        // Assert - Date Information
        Assert.NotNull(result.Today);
        Assert.NotEmpty(result.Today.Date);
        Assert.NotEmpty(result.Today.DayOfWeek);

        Assert.NotNull(result.Tomorrow);
        Assert.NotEmpty(result.Tomorrow.Date);

        Assert.NotNull(result.DayAfterTomorrow);
        Assert.NotEmpty(result.DayAfterTomorrow.Date);

        _output.WriteLine($"\nToday: {result.Today.Date} ({result.Today.DayOfWeek})");
        _output.WriteLine($"  Weekend: {result.Today.IsWeekend}, Holiday: {result.Today.IsHoliday}");
        if (result.Today.IsHoliday)
        {
            _output.WriteLine($"  Holiday Name: {result.Today.HolidayName}");
        }

        // Assert - This Week Information
        Assert.NotNull(result.ThisWeek);
        Assert.NotEmpty(result.ThisWeek.WeekendDays);
        Assert.NotEmpty(result.ThisWeek.WeekendDates);

        _output.WriteLine($"\nThis Week Weekend Days: {string.Join(", ", result.ThisWeek.WeekendDays)}");
        _output.WriteLine($"Remaining Workdays: {result.ThisWeek.RemainingWorkdays.Count}");

        // Assert - Next Week Information
        Assert.NotNull(result.NextWeek);
        Assert.NotEmpty(result.NextWeek.Monday);
        Assert.NotEmpty(result.NextWeek.Friday);
        Assert.NotEmpty(result.NextWeek.Workdays);

        _output.WriteLine($"\nNext Week: {result.NextWeek.Monday} to {result.NextWeek.Friday}");

        // Assert - Key Dates
        Assert.NotNull(result.KeyDates);
        Assert.NotEmpty(result.KeyDates.NextMonday);
        Assert.NotEmpty(result.KeyDates.NextFriday);
        Assert.NotNull(result.KeyDates.NextWeekend);

        _output.WriteLine($"\nKey Dates:");
        _output.WriteLine($"  Next Monday: {result.KeyDates.NextMonday}");
        _output.WriteLine($"  Next Weekend: {result.KeyDates.NextWeekend.Start} to {result.KeyDates.NextWeekend.End}");

        // Assert - Upcoming Holidays
        Assert.NotNull(result.UpcomingHolidays);
        
        if (result.UpcomingHolidays.Any())
        {
            _output.WriteLine($"\nUpcoming Holidays ({result.UpcomingHolidays.Count}):");
            foreach (var holiday in result.UpcomingHolidays.Take(3))
            {
                _output.WriteLine($"  {holiday.Date} - {holiday.Name} ({holiday.DayOfWeek})");
            }
        }
        else
        {
            _output.WriteLine("\nNo upcoming holidays in the next 90 days");
        }

        _output.WriteLine("\n=== Test Passed ===\n");
    }

    [Fact]
    public async Task AnalyzeDateContextAsync_AllLocations_Sequential()
    {
        // This test runs all locations sequentially to avoid API rate limiting
        // and provides a comprehensive report
        
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDateContextService>();

        var locations = TestLocations.Select(data => new 
        { 
            Location = (string)data[0], 
            ExpectedCountry = (string)data[1] 
        }).ToList();

        _output.WriteLine($"=== Testing {locations.Count} Well-Known Locations ===\n");

        var results = new List<(string Location, bool Success, string? Error)>();

        foreach (var loc in locations)
        {
            try
            {
                _output.WriteLine($"Testing: {loc.Location}");
                
                var result = await service.AnalyzeDateContextAsync(loc.Location);
                
                Assert.Equal(loc.ExpectedCountry, result.Location.CountryCode);
                
                _output.WriteLine($"  ✓ Success - {result.Location.ResolvedName} ({result.Location.CountryCode})");
                _output.WriteLine($"    Timezone: {result.Location.Timezone}");
                _output.WriteLine($"    Local Time: {result.Location.CurrentLocalTime}");
                
                results.Add((loc.Location, true, null));
                
                // Small delay to avoid hitting API rate limits
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"  ✗ Failed - {ex.Message}");
                results.Add((loc.Location, false, ex.Message));
            }
        }

        _output.WriteLine($"\n=== Summary ===");
        _output.WriteLine($"Total: {results.Count}");
        _output.WriteLine($"Passed: {results.Count(r => r.Success)}");
        _output.WriteLine($"Failed: {results.Count(r => !r.Success)}");

        if (results.Any(r => !r.Success))
        {
            _output.WriteLine("\nFailed Tests:");
            foreach (var failure in results.Where(r => !r.Success))
            {
                _output.WriteLine($"  - {failure.Location}: {failure.Error}");
            }
        }

        // Assert that all tests passed
        Assert.All(results, r => Assert.True(r.Success, $"Failed: {r.Location} - {r.Error}"));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

/// <summary>
/// Helper record for test results
/// </summary>
public record HolidayInfo(
    string Date,
    string Name,
    string DayOfWeek
);
