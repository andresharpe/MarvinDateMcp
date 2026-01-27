using System.Text.Json.Serialization;

namespace MarvinDateMcp.Api.Models;

// Google Geocoding API response models
public record GoogleGeocodeResponse(
    [property: JsonPropertyName("results")] List<GoogleResult> Results,
    [property: JsonPropertyName("status")] string Status
);

public record GoogleResult(
    [property: JsonPropertyName("address_components")] List<GoogleAddressComponent> AddressComponents,
    [property: JsonPropertyName("formatted_address")] string FormattedAddress,
    [property: JsonPropertyName("geometry")] GoogleGeometry Geometry
);

public record GoogleAddressComponent(
    [property: JsonPropertyName("long_name")] string LongName,
    [property: JsonPropertyName("short_name")] string ShortName,
    [property: JsonPropertyName("types")] List<string> Types
);

public record GoogleGeometry(
    [property: JsonPropertyName("location")] GoogleLocation Location
);

public record GoogleLocation(
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lng")] double Lng
);

// Google Timezone API response models
public record GoogleTimeZoneResponse(
    [property: JsonPropertyName("timeZoneId")] string TimeZoneId,
    [property: JsonPropertyName("timeZoneName")] string TimeZoneName,
    [property: JsonPropertyName("status")] string Status
);

// Internal resolved location model
public record ResolvedLocation(
    string PlaceName,
    string FormattedAddress,
    double Latitude,
    double Longitude,
    string TimeZoneId,
    string CountryCode,
    string? SubdivisionCode
);
