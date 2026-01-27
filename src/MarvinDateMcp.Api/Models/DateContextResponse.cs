namespace MarvinDateMcp.Api.Models;

/// <summary>
/// Comprehensive date context response for LLM consumption
/// </summary>
public record DateContextResponse(
    LocationInfo Location,
    DateInfo Today,
    DateInfo Tomorrow,
    DateInfo DayAfterTomorrow,
    ThisWeekInfo ThisWeek,
    NextWeekInfo NextWeek,
    List<HolidayInfo> UpcomingHolidays,
    KeyDatesInfo KeyDates
);

public record LocationInfo(
    string ResolvedName,
    string FormattedAddress,
    string CountryCode,
    string Timezone,
    string UtcOffset,
    string CurrentLocalTime
);

public record DateInfo(
    string Date,
    string DayOfWeek,
    bool IsWeekend,
    bool IsHoliday,
    string? HolidayName
);

public record ThisWeekInfo(
    List<string> WeekendDays,
    List<string> WeekendDates,
    List<string> RemainingWorkdays
);

public record NextWeekInfo(
    string Monday,
    string Friday,
    List<string> WeekendDates,
    List<string> Workdays
);

public record KeyDatesInfo(
    string NextMonday,
    string NextTuesday,
    string NextWednesday,
    string NextThursday,
    string NextFriday,
    string NextSaturday,
    string NextSunday,
    WeekendRange NextWeekend
);

public record WeekendRange(
    string Start,
    string End
);
