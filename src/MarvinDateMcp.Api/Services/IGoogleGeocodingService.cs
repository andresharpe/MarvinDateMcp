using MarvinDateMcp.Api.Models;

namespace MarvinDateMcp.Api.Services;

public interface IGoogleGeocodingService
{
    /// <summary>
    /// Resolves a place name or POI to full location information including coordinates, timezone, and country
    /// </summary>
    Task<ResolvedLocation> ResolveLocationAsync(string placeName, CancellationToken cancellationToken = default);
}
