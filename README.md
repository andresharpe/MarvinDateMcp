# MarvinDateMcp - Date Context MCP Server

A Model Context Protocol (MCP) server that provides comprehensive date context analysis for location-aware AI applications. Built with .NET 9 and designed to help LLMs understand dates in conversational contexts.

**Production Ready** with enterprise-grade security controls.

## Features

- **Location-aware date analysis**: Accepts place names or POIs (e.g., "Dubai", "London", "Burj Khalifa")
- **Comprehensive date context**: Returns today, tomorrow, day after tomorrow, this week, next week, upcoming holidays, and key dates
- **Regional weekend support**: Handles different weekend schedules (e.g., UAE: Fri-Sat, UK: Sat-Sun)
- **Bank holidays**: Integrates with Nager.Date API for public holidays
- **Timezone handling**: Uses NodaTime for accurate timezone conversions
- **Caching**: In-memory caching for Google API calls and holiday data
- **HTTP transport**: Uses MCP HTTP transport for remote access

## Security Features ✅

- **API Key Authentication**: All MCP endpoints require X-API-Key header
- **Rate Limiting**: 100 requests/minute per IP address
- **Azure Key Vault**: Secrets stored securely with Managed Identity
- **CORS Restrictions**: Configurable allowed origins (no wildcards)
- **Security Headers**: HSTS, CSP, X-Frame-Options, X-XSS-Protection
- **Application Insights**: Comprehensive logging and monitoring
- **IP Allowlisting**: Optional NSG-based network restrictions
- **HTTPS Only**: TLS 1.2+ enforced

See [SECURITY.md](SECURITY.md) for detailed security architecture.

## Setup

### Prerequisites

- .NET 10 SDK
- Google API Key with Geocoding API and Time Zone API enabled

### Local Development

1. **Set up API Keys**:
   - Get Google API key from [Google Cloud Console](https://console.cloud.google.com/)
   - Enable Geocoding API and Time Zone API
   - Generate MCP API key: `openssl rand -base64 32`
   - Create `.env.local` in the project root:
     ```bash
     GOOGLE_API_KEY=your_google_api_key_here
     MCP_API_KEY=your_generated_mcp_key_here
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

### Azure Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for comprehensive deployment guide.

**Quick Deploy:**
```bash
# Configure .env.local with API keys
cd terraform
.\deploy.ps1
```

This will:
- Deploy Azure infrastructure (Key Vault, App Service, Application Insights)
- Build and deploy the application
- Run verification tests

## MCP Tool

### `analyze_date_context`

**Description**: Analyzes comprehensive date context for a location. Returns today, tomorrow, day after tomorrow, this week, next week, upcoming holidays, and key dates.

**Authentication**: Requires `X-API-Key` header with valid MCP API key.

**Input**:
- `location` (string, required): Place name, city, or POI (e.g., "Dubai", "London", "JFK Airport")
- `as_of_date` (string, optional): Date to use as "today" for all calculations, in ISO 8601 format (e.g., "2026-02-15"). If not provided, uses the current date in the location's timezone.

**Example Request**:
```bash
curl -X POST https://YOUR_APP_SERVICE_URL/mcp \
  -H "X-API-Key: your_mcp_api_key" \
  -H "Content-Type: application/json" \
  -H "Mcp-Session-Id: your_session_id" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "analyze_date_context",
      "arguments": {
        "location": "Dubai",
        "as_of_date": "2026-02-15"
      }
    }
  }'
```

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

- .NET 9
- ModelContextProtocol.AspNetCore 0.6.0-preview.1
- NodaTime 3.2.2 (timezone handling)
- Serilog (logging)
- Polly (HTTP resilience)
- Azure Key Vault (secrets management)
- Azure Application Insights (monitoring)
- Google Geocoding API & Time Zone API
- Nager.Date API (bank holidays)

## Documentation

- [SECURITY.md](SECURITY.md) - Security architecture and controls
- [DEPLOYMENT.md](DEPLOYMENT.md) - Deployment guide and troubleshooting
- [.env.example](.env.example) - Environment variables template

## License

MIT
