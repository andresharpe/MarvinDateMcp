# MarvinDateMcp Integration Tests

Integration tests for the DateContextService that test the `AnalyzeDateContextAsync` method with real Google API calls.

## Test Coverage

The test suite includes 11 well-known locations from around the world:

1. **Houses of Parliament, Westminster, London** (GB) - UK Parliament
2. **White House, Washington DC** (US) - US Presidential residence
3. **Eiffel Tower, Paris** (FR) - Iconic French landmark
4. **Tokyo Tower, Tokyo** (JP) - Japanese broadcasting tower
5. **Sydney Opera House, Sydney** (AU) - Australian performing arts center
6. **Brandenburg Gate, Berlin** (DE) - German neoclassical monument
7. **Red Square, Moscow** (RU) - Historic Russian square
8. **Christ the Redeemer, Rio de Janeiro** (BR) - Brazilian Art Deco statue
9. **CN Tower, Toronto** (CA) - Canadian communications tower
10. **Regus Central Station Brussels** (BE) - Coworking center in Brussels
11. **Bleicherweg 10, Zurich** (CH) - Spaces coworking center in Zurich

## Prerequisites

- Google Maps API key configured in `.env.local` in the project root
- The API key must have the following APIs enabled:
  - Geocoding API
  - Time Zone API

## Running the Tests

### Run all tests individually (Theory tests)
```powershell
dotnet test tests/MarvinDateMcp.Tests/MarvinDateMcp.Tests.csproj
```

### Run the sequential test (all locations in one test)
```powershell
dotnet test tests/MarvinDateMcp.Tests/MarvinDateMcp.Tests.csproj --filter "FullyQualifiedName~AllLocations_Sequential"
```

### Run a specific location test
```powershell
dotnet test tests/MarvinDateMcp.Tests/MarvinDateMcp.Tests.csproj --filter "FullyQualifiedName~White House"
```

## What Each Test Validates

Each test verifies:
- ✅ Location is correctly resolved
- ✅ Country code matches expected value
- ✅ Timezone information is present and valid
- ✅ Current local time is calculated correctly
- ✅ Date information (today, tomorrow, day after tomorrow) is accurate
- ✅ Weekend days are correctly identified for the country
- ✅ Holiday information is retrieved
- ✅ This week and next week workdays are calculated
- ✅ Key dates (next Monday, Friday, weekend, etc.) are accurate

## Test Output

Each test provides detailed output including:
- Resolved location name and formatted address
- Country code and timezone
- Current local time at the location
- Today's date and whether it's a weekend/holiday
- Weekend configuration for the country
- Remaining workdays this week
- Next week's schedule
- Upcoming holidays

## Notes

- The tests use real API calls, so they require an internet connection
- A 500ms delay is added between sequential tests to avoid rate limiting
- Tests are marked as integration tests and should be run separately from unit tests
- The `.env.local` file is automatically loaded from the project root
- Holiday lookahead is set to 365 days, with up to 10 holidays displayed per location
