using MarvinDateMcp.Api.Models;

namespace MarvinDateMcp.Api.Services;

public interface IDateContextService
{
    /// <summary>
    /// Analyzes comprehensive date context for a given location
    /// </summary>
    Task<DateContextResponse> AnalyzeDateContextAsync(string location, CancellationToken cancellationToken = default);
}
