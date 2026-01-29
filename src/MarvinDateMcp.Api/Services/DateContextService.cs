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
        var keyDates = new Dictionary<DayOfWeek, DateOnly>();
        
        // Find next occurrence of each day of the week
        foreach (DayOfWeek targetDay in Enum.GetValues<DayOfWeek>())
        {
            var daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0) daysUntilTarget = 7; // Skip to next week if today
            
            keyDates[targetDay] = today.AddDays(daysUntilTarget);
        }
        
        // Find next weekend
        var nextWeekendStart = _weekendService.GetNextWeekend(today, countryCode);
        var weekendDays = _weekendService.GetWeekendDays(countryCode);
        
        // If weekend is 2 days, the end is the day after start (if it's also a weekend day)
        var nextWeekendEnd = nextWeekendStart;
        if (weekendDays.Length == 2)
        {
            var nextDay = nextWeekendStart.AddDays(1);
            if (weekendDays.Contains(nextDay.DayOfWeek))
            {
                nextWeekendEnd = nextDay;
            }
        }
        
        return new KeyDatesInfo(
            keyDates[DayOfWeek.Monday].ToString("yyyy-MM-dd"),
            keyDates[DayOfWeek.Tuesday].ToString("yyyy-MM-dd"),
            keyDates[DayOfWeek.Wednesday].ToString("yyyy-MM-dd"),
            keyDates[DayOfWeek.Thursday].ToString("yyyy-MM-dd"),
            keyDates[DayOfWeek.Friday].ToString("yyyy-MM-dd"),
            keyDates[DayOfWeek.Saturday].ToString("yyyy-MM-dd"),
            keyDates[DayOfWeek.Sunday].ToString("yyyy-MM-dd"),
            new WeekendRange(
                nextWeekendStart.ToString("yyyy-MM-dd"),
                nextWeekendEnd.ToString("yyyy-MM-dd")
            )
        );
    }
}
