using System.Net;
using System.Text.Json;
using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Exceptions;
using MarvinDateMcp.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace MarvinDateMcp.Tests;

public class GoogleGeocodingServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static GoogleGeocodingService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var logger = NullLogger<GoogleGeocodingService>.Instance;
        var googleOptions = Options.Create(new GoogleApiOptions { ApiKey = "test-key" });
        var dateOptions = Options.Create(new DateServiceOptions { GeocodeCacheTtlDays = 7 });
        return new GoogleGeocodingService(httpClient, logger, googleOptions, dateOptions);
    }

    private static string BuildGeocodeOkResponse(string countryCode = "GB", string formattedAddress = "London, UK")
    {
        return JsonSerializer.Serialize(new
        {
            status = "OK",
            results = new[]
            {
                new
                {
                    formatted_address = formattedAddress,
                    geometry = new { location = new { lat = 51.5074, lng = -0.1278 } },
                    address_components = new object[]
                    {
                        new { long_name = "United Kingdom", short_name = countryCode, types = new[] { "country", "political" } },
                        new { long_name = "England", short_name = "ENG", types = new[] { "administrative_area_level_1", "political" } }
                    }
                }
            }
        });
    }

    private static string BuildTimezoneOkResponse()
    {
        return JsonSerializer.Serialize(new
        {
            status = "OK",
            timeZoneId = "Europe/London",
            timeZoneName = "Greenwich Mean Time"
        });
    }

    private static string BuildStatusResponse(string status)
    {
        return JsonSerializer.Serialize(new { status, results = Array.Empty<object>() });
    }

    [Fact]
    public async Task ResolveLocationAsync_SuccessfulResponse_ReturnsResolvedLocation()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildGeocodeOkResponse());
        handler.EnqueueResponse(BuildTimezoneOkResponse());

        var service = CreateService(handler);
        var result = await service.ResolveLocationAsync("London");

        Assert.Equal("London", result.PlaceName);
        Assert.Equal("London, UK", result.FormattedAddress);
        Assert.Equal("GB", result.CountryCode);
        Assert.Equal("Europe/London", result.TimeZoneId);
        Assert.Equal(51.5074, result.Latitude);
        Assert.Equal(-0.1278, result.Longitude);
        Assert.Equal("GB-ENG", result.SubdivisionCode);
    }

    [Fact]
    public async Task ResolveLocationAsync_SuccessfulResponse_IsCached()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildGeocodeOkResponse());
        handler.EnqueueResponse(BuildTimezoneOkResponse());

        var service = CreateService(handler);

        var result1 = await service.ResolveLocationAsync("London");
        var result2 = await service.ResolveLocationAsync("London");

        Assert.Same(result1, result2);
        Assert.Equal(2, handler.RequestCount); // Only the first call should hit the API
    }

    [Fact]
    public async Task ResolveLocationAsync_ZeroResults_ThrowsNotFound()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildStatusResponse("ZERO_RESULTS"));

        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("asdfgjkl"));

        Assert.Equal(LocationResolutionFailureReason.NotFound, ex.Reason);
        Assert.Contains("asdfgjkl", ex.Message);
        Assert.Contains("check the spelling", ex.Message);
    }

    [Fact]
    public async Task ResolveLocationAsync_ZeroResults_NegativeCachePreventsFurtherCalls()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildStatusResponse("ZERO_RESULTS"));

        var service = CreateService(handler);

        await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("badplace"));

        // Second call should hit negative cache, not the API
        await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("badplace"));

        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task ResolveLocationAsync_OverQueryLimit_RetriesThenThrowsRateLimited()
    {
        var handler = new MockHttpMessageHandler();
        // 3 responses: initial + 2 retries, all OVER_QUERY_LIMIT
        handler.EnqueueResponse(BuildStatusResponse("OVER_QUERY_LIMIT"));
        handler.EnqueueResponse(BuildStatusResponse("OVER_QUERY_LIMIT"));
        handler.EnqueueResponse(BuildStatusResponse("OVER_QUERY_LIMIT"));

        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("London"));

        Assert.Equal(LocationResolutionFailureReason.RateLimited, ex.Reason);
        Assert.Contains("temporarily busy", ex.Message);
        Assert.Equal(3, handler.RequestCount); // initial + 2 retries
    }

    [Fact]
    public async Task ResolveLocationAsync_OverQueryLimitThenSuccess_ReturnsResult()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildStatusResponse("OVER_QUERY_LIMIT"));
        handler.EnqueueResponse(BuildGeocodeOkResponse());
        handler.EnqueueResponse(BuildTimezoneOkResponse());

        var service = CreateService(handler);
        var result = await service.ResolveLocationAsync("London");

        Assert.Equal("GB", result.CountryCode);
        Assert.Equal(3, handler.RequestCount); // 1 retry + geocode success + timezone
    }

    [Fact]
    public async Task ResolveLocationAsync_RequestDenied_ThrowsConfigurationError()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildStatusResponse("REQUEST_DENIED"));

        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("London"));

        Assert.Equal(LocationResolutionFailureReason.ConfigurationError, ex.Reason);
        Assert.Contains("configuration error", ex.Message);
        Assert.DoesNotContain("REQUEST_DENIED", ex.Message);
    }

    [Fact]
    public async Task ResolveLocationAsync_NetworkFailure_ThrowsHttpRequestException()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueException(new HttpRequestException("Connection refused"));

        var service = CreateService(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.ResolveLocationAsync("London"));
    }

    [Fact]
    public async Task ResolveLocationAsync_NoCountryInResult_ThrowsNotFound()
    {
        // Return a geocode result with no "country" address component
        var response = JsonSerializer.Serialize(new
        {
            status = "OK",
            results = new[]
            {
                new
                {
                    formatted_address = "Somewhere",
                    geometry = new { location = new { lat = 0.0, lng = 0.0 } },
                    address_components = new[]
                    {
                        new { long_name = "Somewhere", short_name = "SW", types = new[] { "locality" } }
                    }
                }
            }
        });

        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(response);

        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("Somewhere"));

        Assert.Equal(LocationResolutionFailureReason.NotFound, ex.Reason);
        Assert.Contains("country", ex.Message);
    }

    [Fact]
    public async Task ResolveLocationAsync_TimezoneApiFailure_ThrowsWithFriendlyMessage()
    {
        var handler = new MockHttpMessageHandler();
        handler.EnqueueResponse(BuildGeocodeOkResponse());
        handler.EnqueueResponse(JsonSerializer.Serialize(new
        {
            status = "UNKNOWN_ERROR",
            timeZoneId = (string?)null,
            timeZoneName = (string?)null
        }));

        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<LocationResolutionException>(
            () => service.ResolveLocationAsync("London"));

        Assert.Equal(LocationResolutionFailureReason.Unknown, ex.Reason);
        Assert.DoesNotContain("UNKNOWN_ERROR", ex.Message);
        Assert.Contains("London", ex.Message);
    }
}

/// <summary>
/// Simple mock HttpMessageHandler that returns queued responses in order.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responseFactory = new();
    public int RequestCount { get; private set; }

    public void EnqueueResponse(string jsonContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseFactory.Enqueue(() => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        });
    }

    public void EnqueueException(Exception exception)
    {
        _responseFactory.Enqueue(() => throw exception);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;

        if (_responseFactory.Count == 0)
        {
            throw new InvalidOperationException(
                $"No more mock responses queued. Request #{RequestCount}: {request.Method} {request.RequestUri}");
        }

        var factory = _responseFactory.Dequeue();
        return Task.FromResult(factory());
    }
}
