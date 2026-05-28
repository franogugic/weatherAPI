namespace WeatherAPI.Application.Common;

public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
