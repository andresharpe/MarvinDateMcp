using MarvinDateMcp.Api.Configuration;
using MarvinDateMcp.Api.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace MarvinDateMcp.Api.Services;

public class DateContextService : IDateContextService
{
    private readonly IGoogleGeocodingService _geocodingService;
    private readonly IHolidayService _holidayService;
    private readonly WeekendService _weekendService;
    private readonly ILogger<DateContextService> _logger;
    private readonly DateServiceOptions _options;
    
    public DateContextService(
        IGoogleGeocodingService geocodingService,
        IHolidayService holidayService,
        WeekendService weekendService,
        ILogger<DateContextService> logger,
        IOptions<DateServiceOptions> options)
    {
        _geocodingService = geocodingService;
        _holidayService = holidayService;
        _weekendService = weekendService;
        _logger = logger;
        _options = options.Value;
    }
    
    public async Task<DateContextResponse> AnalyzeDateContextAsync(string location, DateOnly? asOfDate = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing date context for location: {Location}, AsOfDate: {AsOfDate}", location, asOfDate?.ToString("yyyy-MM-dd") ?? "today");
        
        // Step 1: Resolve location
        var resolvedLocation = await _geocodingService.ResolveLocationAsync(location, cancellationToken);
        
        // Step 2: Get current time in location's timezone
        var now = SystemClock.Instance.GetCurrentInstant();
        var timezone = DateTimeZoneProviders.Tzdb[resolvedLocation.TimeZoneId];
        var localNow = now.InZone(timezone);
        
        // Use asOfDate if provided, otherwise use actual current date
        var today = asOfDate ?? new DateOnly(localNow.Date.Year, localNow.Date.Month, localNow.Date.Day);
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);
        
        // Step 3: Get holidays for lookahead period
        var lookaheadEnd = today.AddDays(_options.HolidayLookaheadDays);
        var allHolidays = await _holidayService.GetHolidaysAsync(
            resolvedLocation.CountryCode,
            today,
            lookaheadEnd,
            resolvedLocation.SubdivisionCode,
            cancellationToken);
        
        // Step 4: Build date info for today, tomorrow, day after tomorrow
        var todayInfo = await BuildDateInfoAsync(today, resolvedLocation, allHolidays);
        var tomorrowInfo = await BuildDateInfoAsync(tomorrow, resolvedLocation, allHolidays);
        var dayAfterTomorrowInfo = await BuildDateInfoAsync(dayAfterTomorrow, resolvedLocation, allHolidays);
        
        // Step 5: Build this week info
        var thisWeekInfo = BuildThisWeekInfo(today, resolvedLocation.CountryCode);
        
        // Step 6: Build next week info
        var nextWeekInfo = BuildNextWeekInfo(today, resolvedLocation.CountryCode);
        
        // Step 7: Get upcoming holidays (within lookahead period)
        var upcomingHolidays = allHolidays
            .Where(h => h.Date > today)
            .Take(10)
            .Select(h => new HolidayInfo(
                h.Date.ToString("yyyy-MM-dd"),
                h.Name,
                h.Date.DayOfWeek.ToString()))
            .ToList();
        
        // Step 8: Build key dates
        var keyDates = BuildKeyDates(today, resolvedLocation.CountryCode);
        
        // Step 9: Build location info
        var locationInfo = new LocationInfo(
            resolvedLocation.PlaceName,
            resolvedLocation.FormattedAddress,
            resolvedLocation.CountryCode,
            resolvedLocation.TimeZoneId,
            localNow.Offset.ToString("g", null),
            localNow.LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss", null)
        );
        
        return new DateContextResponse(
            locationInfo,
            todayInfo,
            tomorrowInfo,
            dayAfterTomorrowInfo,
            thisWeekInfo,
            nextWeekInfo,
            upcomingHolidays,
            keyDates
        );
    }
    
    private async Task<DateInfo> BuildDateInfoAsync(
        DateOnly date,
        ResolvedLocation location,
        List<PublicHoliday> holidays)
    {
        var isWeekend = _weekendService.IsWeekend(date, location.CountryCode);
        var holiday = holidays.FirstOrDefault(h => h.Date == date);
        
        return new DateInfo(
            date.ToString("yyyy-MM-dd"),
            date.DayOfWeek.ToString(),
            isWeekend,
            holiday != null,
            holiday?.Name
        );
    }
    
    private ThisWeekInfo BuildThisWeekInfo(DateOnly today, string countryCode)
    {
        // Find start and end of this week (Monday to Sunday)
        var daysFromMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
        var monday = today.AddDays(-daysFromMonday);
        var sunday = monday.AddDays(6);
        
        // Get weekend dates for this week
        var weekendDates = _weekendService.GetWeekendDatesInRange(today, sunday, countryCode);
        
        // Get remaining workdays (non-weekend days from today to end of week)
        var remainingWorkdays = new List<DateOnly>();
        for (var date = today; date <= sunday; date = date.AddDays(1))
        {
            if (!_weekendService.IsWeekend(date, countryCode))
            {
                remainingWorkdays.Add(date);
            }
        }
        
        var weekendDayNames = _weekendService.GetWeekendDays(countryCode)
            .Select(d => d.ToString())
            .ToList();
        
        return new ThisWeekInfo(
            weekendDayNames,
            weekendDates.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
            remainingWorkdays.Select(d => d.ToString("yyyy-MM-dd")).ToList()
        );
    }
    
    private NextWeekInfo BuildNextWeekInfo(DateOnly today, string countryCode)
    {
        // Find next Monday
        var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilNextMonday == 0) daysUntilNextMonday = 7;
        
        var nextMonday = today.AddDays(daysUntilNextMonday);
        var nextFriday = nextMonday.AddDays(4);
        var nextSunday = nextMonday.AddDays(6);
        
        // Get weekend dates for next week
        var weekendDates = _weekendService.GetWeekendDatesInRange(nextMonday, nextSunday, countryCode);
        
        // Get workdays for next week
        var workdays = new List<DateOnly>();
        for (var date = nextMonday; date <= nextSunday; date = date.AddDays(1))
        {
            if (!_weekendService.IsWeekend(date, countryCode))
            {
                workdays.Add(date);
            }
        }
        
        return new NextWeekInfo(
            nextMonday.ToString("yyyy-MM-dd"),
            nextFriday.ToString("yyyy-MM-dd"),
            weekendDates.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
            workdays.Select(d => d.ToString("yyyy-MM-dd")).ToList()
        );
    }
    
    private KeyDatesInfo BuildKeyDates(DateOnly today, string countryCode)
    {
        // Calculate next week's Monday (the full calendar week after this one)
        var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilNextMonday == 0) daysUntilNextMonday = 7;
        var nextWeekMonday = today.AddDays(daysUntilNextMonday);
        
        // All days are relative to next week's Monday
        var nextWeekTuesday = nextWeekMonday.AddDays(1);
        var nextWeekWednesday = nextWeekMonday.AddDays(2);
        var nextWeekThursday = nextWeekMonday.AddDays(3);
        var nextWeekFriday = nextWeekMonday.AddDays(4);
        var nextWeekSaturday = nextWeekMonday.AddDays(5);
        var nextWeekSunday = nextWeekMonday.AddDays(6);
        
        // Next week's weekend dates
        var weekendDays = _weekendService.GetWeekendDays(countryCode);
        var nextWeekWeekendDates = new List<DateOnly>();
        for (var date = nextWeekMonday; date <= nextWeekSunday; date = date.AddDays(1))
        {
            if (weekendDays.Contains(date.DayOfWeek))
            {
                nextWeekWeekendDates.Add(date);
            }
        }
        
        var weekendStart = nextWeekWeekendDates.FirstOrDefault();
        var weekendEnd = nextWeekWeekendDates.LastOrDefault();
        
        return new KeyDatesInfo(
            nextWeekMonday.ToString("yyyy-MM-dd"),
            nextWeekTuesday.ToString("yyyy-MM-dd"),
            nextWeekWednesday.ToString("yyyy-MM-dd"),
            nextWeekThursday.ToString("yyyy-MM-dd"),
            nextWeekFriday.ToString("yyyy-MM-dd"),
            nextWeekSaturday.ToString("yyyy-MM-dd"),
            nextWeekSunday.ToString("yyyy-MM-dd"),
            new WeekendRange(
                weekendStart.ToString("yyyy-MM-dd"),
                weekendEnd.ToString("yyyy-MM-dd")
            )
        );
    }
}
