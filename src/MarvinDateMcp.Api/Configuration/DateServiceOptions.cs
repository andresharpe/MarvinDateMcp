namespace MarvinDateMcp.Api.Configuration;

public class DateServiceOptions
{
    public const string SectionName = "DateService";
    
    public int HolidayCacheTtlDays { get; set; } = 30;
    public int GeocodeCacheTtlDays { get; set; } = 7;
    public int HolidayLookaheadDays { get; set; } = 90;
}
