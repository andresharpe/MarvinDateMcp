using MarvinDateMcp.Api.Models;

namespace MarvinDateMcp.Api.Services;

public interface IHolidayService
{
    /// <summary>
    /// Gets holidays for a country within a date range, filtered by subdivision if applicable
    /// </summary>
    Task<List<PublicHoliday>> GetHolidaysAsync(
        string countryCode, 
        DateOnly startDate, 
        DateOnly endDate,
        string? subdivisionCode = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a specific date is a holiday for the given country/subdivision
    /// </summary>
    Task<PublicHoliday?> GetHolidayForDateAsync(
        string countryCode,
        DateOnly date,
        string? subdivisionCode = null,
        CancellationToken cancellationToken = default);
}
