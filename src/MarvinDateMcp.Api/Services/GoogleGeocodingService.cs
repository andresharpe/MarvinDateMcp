using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Exceptions;
using MarvinDateMcp.Api.Models;
using Microsoft.Extensions.Options;

namespace MarvinDateMcp.Api.Services;

public class GoogleGeocodingService : IGoogleGeocodingService
{
    private const int OverQueryLimitMaxRetries = 2;
    private static readonly TimeSpan OverQueryLimitRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan NegativeCacheTtl = TimeSpan.FromHours(1);
    
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleGeocodingService> _logger;
    private readonly GoogleApiOptions _googleOptions;
    private readonly DateServiceOptions _dateOptions;
    
    // Simple in-memory cache: place name -> resolved location
    private readonly ConcurrentDictionary<string, CachedLocation> _cache = new();
    
    // Negative cache: place name -> expiry time (for ZERO_RESULTS)
    private readonly ConcurrentDictionary<string, DateTime> _negativeCache = new();
    
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
        
        // Check negative cache
        if (_negativeCache.TryGetValue(cacheKey, out var negativeExpiry))
        {
            if (DateTime.UtcNow < negativeExpiry)
            {
                _logger.LogDebug("Negative cache hit for {PlaceName}", placeName);
                throw new LocationResolutionException(
                    $"Could not find location '{placeName}'. Please check the spelling or try a more specific place name.",
                    LocationResolutionFailureReason.NotFound);
            }
            
            _negativeCache.TryRemove(cacheKey, out _);
        }
        
        // Check positive cache
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow < cached.ExpiresAt)
            {
                _logger.LogDebug("Using cached location for {PlaceName}", placeName);
                return cached.Location;
            }
            
            _cache.TryRemove(cacheKey, out _);
        }
        
        _logger.LogInformation("Resolving location for {PlaceName}", placeName);
        
        // Step 1: Geocode to get lat/lng and country
        var geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(placeName)}&key={_googleOptions.ApiKey}";
        
        var geocodeResponse = await CallGoogleApiWithRetry<GoogleGeocodeResponse>(geocodeUrl, cancellationToken)
            ?? throw new LocationResolutionException(
                $"Location service returned an unexpected error for '{placeName}'.",
                LocationResolutionFailureReason.ServiceUnavailable);
        
        HandleGoogleStatus(geocodeResponse.Status, placeName, cacheKey);
        
        if (geocodeResponse.Results.Count == 0)
        {
            CacheNegativeResult(cacheKey);
            throw new LocationResolutionException(
                $"Could not find location '{placeName}'. Please check the spelling or try a more specific place name.",
                LocationResolutionFailureReason.NotFound);
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
            throw new LocationResolutionException(
                $"Could not determine the country for location '{placeName}'. Please try a more specific place name.",
                LocationResolutionFailureReason.NotFound);
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
        
        var timezoneResponse = await CallGoogleApiWithRetry<GoogleTimeZoneResponse>(timezoneUrl, cancellationToken)
            ?? throw new LocationResolutionException(
                $"Location service returned an unexpected error for '{placeName}'.",
                LocationResolutionFailureReason.ServiceUnavailable);
        
        HandleGoogleStatus(timezoneResponse.Status, placeName, cacheKey: null);
        
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
    
    private async Task<T?> CallGoogleApiWithRetry<T>(string url, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt <= OverQueryLimitMaxRetries; attempt++)
        {
            var response = await _httpClient.GetFromJsonAsync<T>(url, cancellationToken);
            
            // Check if the response has a Status property indicating OVER_QUERY_LIMIT
            var status = response switch
            {
                GoogleGeocodeResponse g => g.Status,
                GoogleTimeZoneResponse t => t.Status,
                _ => null
            };
            
            if (!string.Equals(status, "OVER_QUERY_LIMIT", StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }
            
            if (attempt < OverQueryLimitMaxRetries)
            {
                _logger.LogWarning("Google API returned OVER_QUERY_LIMIT, retrying in {Delay}s (attempt {Attempt}/{MaxRetries})",
                    OverQueryLimitRetryDelay.TotalSeconds, attempt + 1, OverQueryLimitMaxRetries);
                await Task.Delay(OverQueryLimitRetryDelay, cancellationToken);
            }
        }
        
        // All retries exhausted for OVER_QUERY_LIMIT
        throw new LocationResolutionException(
            "Location service is temporarily busy. Please try again in a moment.",
            LocationResolutionFailureReason.RateLimited);
    }
    
    private void HandleGoogleStatus(string status, string placeName, string? cacheKey)
    {
        if (string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        
        switch (status.ToUpperInvariant())
        {
            case "ZERO_RESULTS":
                if (cacheKey != null) CacheNegativeResult(cacheKey);
                throw new LocationResolutionException(
                    $"Could not find location '{placeName}'. Please check the spelling or try a more specific place name.",
                    LocationResolutionFailureReason.NotFound);
            
            case "OVER_QUERY_LIMIT":
                // Should not reach here if CallGoogleApiWithRetry handled it, but just in case
                throw new LocationResolutionException(
                    "Location service is temporarily busy. Please try again in a moment.",
                    LocationResolutionFailureReason.RateLimited);
            
            case "REQUEST_DENIED":
                _logger.LogError("Google API request denied for {PlaceName} - check API key configuration", placeName);
                throw new LocationResolutionException(
                    "Location service configuration error. Please contact support.",
                    LocationResolutionFailureReason.ConfigurationError);
            
            default:
                _logger.LogError("Google API returned unexpected status {Status} for {PlaceName}", status, placeName);
                throw new LocationResolutionException(
                    $"Location service returned an unexpected error for '{placeName}'.",
                    LocationResolutionFailureReason.Unknown);
        }
    }
    
    private void CacheNegativeResult(string cacheKey)
    {
        _negativeCache[cacheKey] = DateTime.UtcNow.Add(NegativeCacheTtl);
        _logger.LogDebug("Cached negative result for {CacheKey} (TTL: {Ttl})", cacheKey, NegativeCacheTtl);
    }
    
    private record CachedLocation(ResolvedLocation Location, DateTime ExpiresAt);
}
