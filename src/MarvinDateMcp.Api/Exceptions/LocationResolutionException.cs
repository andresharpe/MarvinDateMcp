namespace MarvinDateMcp.Api.Exceptions;

public enum LocationResolutionFailureReason
{
    NotFound,
    RateLimited,
    ServiceUnavailable,
    ConfigurationError,
    Unknown
}

public class LocationResolutionException : Exception
{
    public LocationResolutionFailureReason Reason { get; }

    public LocationResolutionException(string message, LocationResolutionFailureReason reason)
        : base(message)
    {
        Reason = reason;
    }

    public LocationResolutionException(string message, LocationResolutionFailureReason reason, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
    }
}
