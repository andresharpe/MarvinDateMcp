using System.ComponentModel;
using System.Text.Json;
using MarvinDateMcp.Api.Services;
using ModelContextProtocol.Server;

namespace MarvinDateMcp.Api.Tools;

[McpServerToolType]
public sealed class DateContextTool
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    [McpServerTool, Description(@"Analyzes comprehensive date context for a location. Returns today, tomorrow, day after tomorrow, this week, next week, upcoming holidays, and key dates (next Monday-Sunday, next weekend). Handles location-specific weekends (e.g., Dubai weekend is Friday-Saturday, UK weekend is Saturday-Sunday) and bank holidays. Use this to answer questions like 'Can I tour tomorrow?', 'How about next week?', 'Day after tomorrow?', 'What about Friday?'.")]
    public static async Task<string> AnalyzeDateContext(
        [Description("The place name, city, or point of interest to analyze dates for (e.g., 'Dubai', 'London', 'JFK Airport', 'Burj Khalifa')")]
        string location,
        [Description("Optional date to use as 'today' for all calculations, in ISO 8601 format (e.g., '2026-02-15'). If not provided, uses the current date in the location's timezone.")]
        string? as_of_date,
        ILogger<DateContextTool> logger,
        IServiceProvider serviceProvider)
    {
        logger.LogInformation("---> Analyzing date context for location: {Location}, AsOfDate: {AsOfDate}", location, as_of_date ?? "today");
        
        try
        {
            // Parse as_of_date if provided
            DateOnly? asOfDate = null;
            if (!string.IsNullOrWhiteSpace(as_of_date))
            {
                if (!DateOnly.TryParse(as_of_date, out var parsedDate))
                {
                    return JsonSerializer.Serialize(new { error = $"Invalid date format: '{as_of_date}'. Please use ISO 8601 format (e.g., '2026-02-15')." }, JsonOptions);
                }
                asOfDate = parsedDate;
            }
            
            var dateContextService = serviceProvider.GetRequiredService<IDateContextService>();
            var response = await dateContextService.AnalyzeDateContextAsync(location, asOfDate);
            
            var result = JsonSerializer.Serialize(response, JsonOptions);
            
            logger.LogInformation("Date context analysis complete for {Location}", location);
            
            return result;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to analyze date context for {Location}", location);
            return JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error analyzing date context for {Location}", location);
            return JsonSerializer.Serialize(new { error = "An unexpected error occurred. Please try again." }, JsonOptions);
        }
    }
}
