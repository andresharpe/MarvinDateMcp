using System.Collections.Concurrent;
using System.Net.Http.Json;
using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Models;
using Microsoft.Extensions.Options;

namespace MarvinDateMcp.Api.Services;

public class HolidayService : IHolidayService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HolidayService> _logger;
    private readonly DateServiceOptions _options;
    
    // Cache: country-year -> list of holidays
    private readonly ConcurrentDictionary<string, CachedHolidays> _cache = new();
    
    public HolidayService(
        HttpClient httpClient,
        ILogger<HolidayService> logger,
        IOptions<DateServiceOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }
    
    public async Task<List<PublicHoliday>> GetHolidaysAsync(
        string countryCode,
        DateOnly startDate,
        DateOnly endDate,
        string? subdivisionCode = null,
        CancellationToken cancellationToken = default)
    {
        var result = new List<PublicHoliday>();
        
        // Fetch holidays for each year in the range
        for (var year = startDate.Year; year <= endDate.Year; year++)
        {
            var yearHolidays = await GetHolidaysForYearAsync(countryCode, year, cancellationToken);
            
            result.AddRange(yearHolidays
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .Where(h => AppliesToSubdivision(h, subdivisionCode)));
        }
        
        return result.OrderBy(h => h.Date).ToList();
    }
    
    public async Task<PublicHoliday?> GetHolidayForDateAsync(
        string countryCode,
        DateOnly date,
        string? subdivisionCode = null,
        CancellationToken cancellationToken = default)
    {
        var yearHolidays = await GetHolidaysForYearAsync(countryCode, date.Year, cancellationToken);
        
        return yearHolidays
            .Where(h => h.Date == date)
            .FirstOrDefault(h => AppliesToSubdivision(h, subdivisionCode));
    }
    
    private async Task<List<PublicHoliday>> GetHolidaysForYearAsync(
        string countryCode,
        int year,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{countryCode}-{year}";
        
        // Check cache
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow < cached.ExpiresAt)
            {
                _logger.LogDebug("Using cached holidays for {Country} {Year}", countryCode, year);
                return cached.Holidays;
            }
            
            _cache.TryRemove(cacheKey, out _);
        }
        
        _logger.LogInformation("Fetching holidays for {Country} {Year}", countryCode, year);
        
        try
        {
            var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";
            var holidays = await _httpClient.GetFromJsonAsync<List<PublicHoliday>>(url, cancellationToken)
                ?? [];
            
            // Cache the result
            _cache[cacheKey] = new CachedHolidays(
                holidays,
                DateTime.UtcNow.AddDays(_options.HolidayCacheTtlDays)
            );
            
            _logger.LogInformation("Cached {Count} holidays for {Country} {Year}", 
                holidays.Count, countryCode, year);
            
            return holidays;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("No holidays found for {Country} {Year}", countryCode, year);
            
            // Cache empty result to avoid repeated API calls
            _cache[cacheKey] = new CachedHolidays(
                [],
                DateTime.UtcNow.AddDays(_options.HolidayCacheTtlDays)
            );
            
            return [];
        }
    }
    
    private static bool AppliesToSubdivision(PublicHoliday holiday, string? subdivisionCode)
    {
        // If holiday has no county restrictions, it applies everywhere
        if (holiday.Counties == null || holiday.Counties.Count == 0)
            return true;
        
        // If we don't have subdivision info, assume it applies
        if (string.IsNullOrEmpty(subdivisionCode))
            return true;
        
        // Check if this subdivision is in the holiday's applicable counties
        return holiday.Counties.Contains(subdivisionCode, StringComparer.OrdinalIgnoreCase);
    }
    
    private record CachedHolidays(List<PublicHoliday> Holidays, DateTime ExpiresAt);
}
