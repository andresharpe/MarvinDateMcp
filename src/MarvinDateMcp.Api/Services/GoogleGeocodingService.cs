using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Models;
using Microsoft.Extensions.Options;

namespace MarvinDateMcp.Api.Services;

public class GoogleGeocodingService : IGoogleGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleGeocodingService> _logger;
    private readonly GoogleApiOptions _googleOptions;
    private readonly DateServiceOptions _dateOptions;
    
    // Simple in-memory cache: place name -> resolved location
    private readonly ConcurrentDictionary<string, CachedLocation> _cache = new();
    
    public GoogleGeocodingService(
        HttpClient httpClient,
        ILogger<GoogleGeocodingService> logger,
        IOptions<GoogleApiOptions> googleOptions,
        IOptions<DateServiceOptions> dateOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _googleOptions = googleOptions.Value;
        _dateOptions = dateOptions.Value;
    }
    
    public async Task<ResolvedLocation> ResolveLocationAsync(string placeName, CancellationToken cancellationToken = default)
    {
        var cacheKey = placeName.ToLowerInvariant();
        
        // Check cache
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow < cached.ExpiresAt)
            {
                _logger.LogDebug("Using cached location for {PlaceName}", placeName);
                return cached.Location;
            }
            
            // Remove expired entry
            _cache.TryRemove(cacheKey, out _);
        }
        
        _logger.LogInformation("Resolving location for {PlaceName}", placeName);
        
        // Step 1: Geocode to get lat/lng and country
        var geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(placeName)}&key={_googleOptions.ApiKey}";
        
        var geocodeResponse = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(geocodeUrl, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to geocode location: {placeName}");
        
        if (!string.Equals(geocodeResponse.Status, "OK", StringComparison.OrdinalIgnoreCase) || geocodeResponse.Results.Count == 0)
        {
            throw new InvalidOperationException($"Google Geocoding API returned status: {geocodeResponse.Status} for location: {placeName}");
        }
        
        var result = geocodeResponse.Results[0];
        var lat = result.Geometry.Location.Lat;
        var lng = result.Geometry.Location.Lng;
        var formattedAddress = result.FormattedAddress;
        
        // Extract country code
        var countryComponent = result.AddressComponents
            .FirstOrDefault(c => c.Types.Contains("country"));
        
        if (countryComponent == null)
        {
            throw new InvalidOperationException($"No country found for location: {placeName}");
        }
        
        var countryCode = countryComponent.ShortName.ToUpperInvariant();
        
        // Extract subdivision (state/province) if available
        var subdivisionComponent = result.AddressComponents
            .FirstOrDefault(c => c.Types.Contains("administrative_area_level_1"));
        
        string? subdivisionCode = null;
        if (subdivisionComponent != null)
        {
            var adminCode = subdivisionComponent.ShortName.ToUpperInvariant();
            if (adminCode.Length <= 3 && adminCode.All(char.IsLetterOrDigit))
            {
                subdivisionCode = $"{countryCode}-{adminCode}";
            }
        }
        
        // Step 2: Get timezone
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timezoneUrl = $"https://maps.googleapis.com/maps/api/timezone/json?location={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}&timestamp={unixTimestamp}&key={_googleOptions.ApiKey}";
        
        var timezoneResponse = await _httpClient.GetFromJsonAsync<GoogleTimeZoneResponse>(timezoneUrl, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to get timezone for location: {placeName}");
        
        if (!string.Equals(timezoneResponse.Status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Google Timezone API returned status: {timezoneResponse.Status} for location: {placeName}");
        }
        
        var resolvedLocation = new ResolvedLocation(
            placeName,
            formattedAddress,
            lat,
            lng,
            timezoneResponse.TimeZoneId,
            countryCode,
            subdivisionCode
        );
        
        // Cache the result
        var cachedLocation = new CachedLocation(
            resolvedLocation,
            DateTime.UtcNow.AddDays(_dateOptions.GeocodeCacheTtlDays)
        );
        
        _cache[cacheKey] = cachedLocation;
        
        _logger.LogInformation("Resolved {PlaceName} to {Country} ({TimeZone})", 
            placeName, countryCode, timezoneResponse.TimeZoneId);
        
        return resolvedLocation;
    }
    
    private record CachedLocation(ResolvedLocation Location, DateTime ExpiresAt);
}
