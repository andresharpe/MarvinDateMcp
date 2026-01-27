using System.Text.Json.Serialization;

namespace MarvinDateMcp.Api.Models;

// Nager.Date API response model
public record PublicHoliday(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("localName")] string LocalName,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("countryCode")] string CountryCode,
    [property: JsonPropertyName("counties")] List<string>? Counties
);

// Simplified holiday info for the response
public record HolidayInfo(
    string Date,
    string Name,
    string DayOfWeek
);
