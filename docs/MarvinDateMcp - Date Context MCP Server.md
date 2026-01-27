# MarvinDateMcp - Date Context MCP Server
## Problem Statement
Build an MCP server that helps LLMs understand dates in conversational contexts. Customers ask questions like "Can I tour tomorrow?", "How about next week?", "Day after tomorrow?" and the LLM needs accurate, location-aware date information including:
* What day a date falls on
* Whether it's a local weekend (Dubai: Fri-Sat, UK: Sat-Sun)
* Local time vs GMT
* Bank holidays
* Date math ("next Thursday", "next week")
## Architecture Decisions
* **.NET 10** with **HTTP transport** (like Marvin.Api)
* **Single comprehensive tool** `analyze_date_context` returns all date info in one call
* **Place name/POI input** - geocoded via Google Maps API (like Marvin)
* **Self-contained** - own Google API integration with caching (like WorkaApp)
* **NodaTime 3.2.2** for timezone handling (same as WorkaApp)
* **Nager.Date API** for bank holidays with caching
* **Custom weekend rules** for UAE (Fri-Sat), Saudi Arabia (Fri-Sat), Israel (Fri-Sat), Western countries (Sat-Sun)
## Project Structure
```warp-runnable-command
MarvinDateMcp/
├── MarvinDateMcp.sln
├── .env.local (gitignored - Google API key)
├── src/
│   └── MarvinDateMcp.Api/
│       ├── Program.cs
│       ├── MarvinDateMcp.Api.csproj
│       ├── appsettings.json
│       ├── Configuration/
│       │   ├── GoogleApiOptions.cs
│       │   └── DateServiceOptions.cs
│       ├── Models/
│       │   ├── DateContextResponse.cs
│       │   ├── GoogleApiModels.cs
│       │   └── PublicHolidayModels.cs
│       ├── Services/
│       │   ├── IDateContextService.cs
│       │   ├── DateContextService.cs
│       │   ├── IGoogleGeocodingService.cs
│       │   ├── GoogleGeocodingService.cs
│       │   ├── IHolidayService.cs
│       │   ├── HolidayService.cs
│       │   └── WeekendService.cs
│       └── Tools/
│           └── DateContextTool.cs
```
## Key NuGet Packages
* `ModelContextProtocol.AspNetCore` (latest prerelease) - MCP HTTP transport
* `NodaTime` 3.2.2 - timezone handling (same as WorkaApp)
* `Microsoft.Extensions.Hosting` - hosting
* `Microsoft.Extensions.Http.Polly` - HTTP resilience
* `Serilog.AspNetCore` - logging
## Tool Design: `analyze_date_context`
**Input:**
* `location` (string, required): Place name or POI (e.g., "Dubai", "London Bridge", "JFK Airport")
**Output (DateContextResponse):**
```json
{
  "location": {
    "resolvedName": "Dubai, United Arab Emirates",
    "countryCode": "AE",
    "timezone": "Asia/Dubai",
    "utcOffset": "+04:00",
    "currentLocalTime": "2026-01-27T15:22:00"
  },
  "today": {
    "date": "2026-01-27",
    "dayOfWeek": "Tuesday",
    "isWeekend": false,
    "isHoliday": false,
    "holidayName": null
  },
  "tomorrow": { ... },
  "dayAfterTomorrow": { ... },
  "thisWeek": {
    "weekendDays": ["Friday", "Saturday"],
    "weekendDates": ["2026-01-30", "2026-01-31"],
    "remainingWorkdays": ["2026-01-28", "2026-01-29"]
  },
  "nextWeek": {
    "monday": "2026-02-02",
    "friday": "2026-02-06",
    "weekendDates": ["2026-02-06", "2026-02-07"],
    "workdays": ["2026-02-01", "2026-02-02", "2026-02-03", "2026-02-04", "2026-02-05"]
  },
  "upcomingHolidays": [
    { "date": "2026-02-10", "name": "Isra and Mi'raj", "dayOfWeek": "Tuesday" }
  ],
  "keyDates": {
    "nextMonday": "2026-02-02",
    "nextFriday": "2026-01-30",
    "nextWeekend": { "start": "2026-01-30", "end": "2026-01-31" }
  }
}
```
## Weekend Rules Configuration
```csharp
public static class WeekendRules
{
    public static readonly Dictionary<string, DayOfWeek[]> CountryWeekends = new()
    {
        ["AE"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // UAE
        ["SA"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Saudi Arabia
        ["IL"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Israel
        // All others default to Saturday-Sunday
    };
}
```
## Services Overview
### GoogleGeocodingService
* Geocodes place name → lat/lng
* Gets timezone via Google Timezone API
* Gets country code via Google Geocoding API
* Simple in-memory cache (place names don't change)
### HolidayService
* Fetches holidays from Nager.Date API
* In-memory cache with TTL (holidays for a year don't change often)
* Filters by country and optional subdivision
### WeekendService
* Determines weekend days based on country code
* Returns whether a date is a weekend
### DateContextService
* Orchestrates all services
* Builds the comprehensive DateContextResponse
* Handles date math (next Monday, next week, etc.)
## Implementation Steps
1. Create solution and project structure
2. Add NuGet packages
3. Implement configuration classes
4. Implement Google API models and service
5. Implement holiday models and service
6. Implement weekend service
7. Implement DateContextService
8. Implement MCP tool
9. Wire up Program.cs with DI and MCP
10. Test with MCP inspector
## Configuration (appsettings.json)
```json
{
  "Google": {
    "ApiKey": "(from .env.local)"
  },
  "DateService": {
    "HolidayCacheTtlDays": 30,
    "GeocodeCacheTtlDays": 7,
    "HolidayLookaheadDays": 90
  }
}
```
