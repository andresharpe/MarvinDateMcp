namespace MarvinDateMcp.Api.Configuration;

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Maximum number of requests allowed per window per IP
    /// Default: 100 for development, recommend 5000+ for production with ElevenLabs
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Time window in minutes for rate limiting
    /// </summary>
    public int WindowMinutes { get; set; } = 1;

    /// <summary>
    /// Maximum number of requests that can be queued when limit is reached
    /// 0 = no queuing, requests are immediately rejected
    /// </summary>
    public int QueueLimit { get; set; } = 0;
}
