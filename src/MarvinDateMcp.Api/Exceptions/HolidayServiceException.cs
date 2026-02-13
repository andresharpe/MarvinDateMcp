namespace MarvinDateMcp.Api.Exceptions;

public class HolidayServiceException : Exception
{
    public string CountryCode { get; }

    public HolidayServiceException(string message, string countryCode)
        : base(message)
    {
        CountryCode = countryCode;
    }

    public HolidayServiceException(string message, string countryCode, Exception innerException)
        : base(message, innerException)
    {
        CountryCode = countryCode;
    }
}
