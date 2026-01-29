using MarvinDateMcp.Api.Models;

namespace MarvinDateMcp.Api.Services;

public interface IDateContextService
{
    /// <summary>
    /// Analyzes comprehensive date context for a given location
    /// </summary>
    /// <param name="location">The location to analyze</param>
    /// <param name="asOfDate">Optional date to use as "today" for calculations. Defaults to actual current date if not specified.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<DateContextResponse> AnalyzeDateContextAsync(string location, DateOnly? asOfDate = null, CancellationToken cancellationToken = default);
}
