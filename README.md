# MarvinDateMcp - Date Context MCP Server

A Model Context Protocol (MCP) server that provides comprehensive date context analysis for location-aware AI applications. Built with .NET 10 and designed to help LLMs understand dates in conversational contexts.

## Features

- **Location-aware date analysis**: Accepts place names or POIs (e.g., "Dubai", "London", "Burj Khalifa")
- **Comprehensive date context**: Returns today, tomorrow, day after tomorrow, this week, next week, upcoming holidays, and key dates
- **Regional weekend support**: Handles different weekend schedules (e.g., UAE: Fri-Sat, UK: Sat-Sun)
- **Bank holidays**: Integrates with Nager.Date API for public holidays
- **Timezone handling**: Uses NodaTime for accurate timezone conversions
- **Caching**: In-memory caching for Google API calls and holiday data
- **HTTP transport**: Uses MCP HTTP transport for remote access

## Setup

### Prerequisites

- .NET 10 SDK
- Google API Key with Geocoding API and Time Zone API enabled

### Configuration

1. **Set up Google API Key**:
   - Get an API key from [Google Cloud Console](https://console.cloud.google.com/)
   - Enable Geocoding API and Time Zone API
   - Edit `.env.local` in the project root:
     ```
     GOOGLE_API_KEY=your_actual_api_key_here
     ```

2. **Build the project**:
   ```bash
   dotnet build
   ```

3. **Run the server**:
   ```bash
   dotnet run --project src/MarvinDateMcp.Api/MarvinDateMcp.Api.csproj
   ```

The server will start on `http://localhost:5000` (or configured port).

## MCP Tool

### `analyze_date_context`

**Description**: Analyzes comprehensive date context for a location. Returns today, tomorrow, day after tomorrow, this week, next week, upcoming holidays, and key dates.

**Input**:
- `location` (string): Place name, city, or POI (e.g., "Dubai", "London", "JFK Airport")

**Output**: JSON with comprehensive date information:

```json
{
  "location": {
    "resolvedName": "Dubai",
    "formattedAddress": "Dubai, United Arab Emirates",
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
    "nextTuesday": "2026-01-28",
    "nextWednesday": "2026-01-29",
    "nextThursday": "2026-01-30",
    "nextFriday": "2026-01-31",
    "nextSaturday": "2026-02-01",
    "nextSunday": "2026-02-08",
    "nextWeekend": { "start": "2026-01-30", "end": "2026-01-31" }
  }
}
```

## Use Cases

The LLM can use this tool to answer questions like:
- "Can I tour tomorrow?"
- "How about next week?"
- "Day after tomorrow?"
- "What about Friday?"
- "Is Monday a holiday?"
- "When is the next weekend?"

## Weekend Rules

The server supports location-specific weekends:
- **UAE, Saudi Arabia, Israel, Bahrain, Kuwait, Oman, Qatar**: Friday-Saturday
- **All other countries**: Saturday-Sunday

## Architecture

- **GoogleGeocodingService**: Resolves place names to coordinates, timezones, and country codes
- **HolidayService**: Fetches holidays from Nager.Date API with caching
- **WeekendService**: Determines weekend days based on country
- **DateContextService**: Orchestrates all services to build comprehensive response
- **DateContextTool**: MCP tool that exposes the service to LLMs

## Endpoints

- `/mcp` - MCP server endpoint
- `/health` - Health check endpoint
- `/` - Simple homepage

## Technologies

- .NET 10
- ModelContextProtocol.AspNetCore 0.6.0-preview.1
- NodaTime 3.2.2 (timezone handling)
- Serilog (logging)
- Polly (HTTP resilience)
- Google Geocoding API & Time Zone API
- Nager.Date API (bank holidays)

## License

MIT
