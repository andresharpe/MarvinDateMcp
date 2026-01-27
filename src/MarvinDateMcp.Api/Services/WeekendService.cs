namespace MarvinDateMcp.Api.Services;

public class WeekendService
{
    // Weekend rules by country code
    private static readonly Dictionary<string, DayOfWeek[]> CountryWeekends = new()
    {
        ["AE"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // UAE
        ["SA"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Saudi Arabia
        ["IL"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Israel
        ["BH"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Bahrain
        ["KW"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Kuwait
        ["OM"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Oman
        ["QA"] = [DayOfWeek.Friday, DayOfWeek.Saturday],     // Qatar
        // Default for all other countries is Saturday-Sunday
    };
    
    // Default weekend days (Saturday-Sunday) for most countries
    private static readonly DayOfWeek[] DefaultWeekend = [DayOfWeek.Saturday, DayOfWeek.Sunday];
    
    public DayOfWeek[] GetWeekendDays(string countryCode)
    {
        return CountryWeekends.TryGetValue(countryCode.ToUpperInvariant(), out var weekend)
            ? weekend
            : DefaultWeekend;
    }
    
    public bool IsWeekend(DateOnly date, string countryCode)
    {
        var weekendDays = GetWeekendDays(countryCode);
        return weekendDays.Contains(date.DayOfWeek);
    }
    
    public List<DateOnly> GetWeekendDatesInRange(DateOnly start, DateOnly end, string countryCode)
    {
        var weekendDays = GetWeekendDays(countryCode);
        var result = new List<DateOnly>();
        
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (weekendDays.Contains(date.DayOfWeek))
            {
                result.Add(date);
            }
        }
        
        return result;
    }
    
    public DateOnly GetNextWeekend(DateOnly fromDate, string countryCode)
    {
        var weekendDays = GetWeekendDays(countryCode);
        var date = fromDate.AddDays(1);
        
        while (!weekendDays.Contains(date.DayOfWeek))
        {
            date = date.AddDays(1);
        }
        
        return date;
    }
}
