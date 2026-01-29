namespace MarvinDateMcp.Api.Configuration;

/// <summary>
/// Configuration options for HTTP retry policies
/// </summary>
public class RetryPolicyOptions
{
    public const string SectionName = "RetryPolicy";

    /// <summary>
    /// Number of retry attempts for transient HTTP errors
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Base delay in seconds for exponential backoff
    /// Actual delay = BaseDelaySeconds ^ retryAttempt
    /// </summary>
    public int BaseDelaySeconds { get; set; } = 2;
}
