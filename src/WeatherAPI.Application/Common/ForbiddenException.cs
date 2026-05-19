namespace WeatherAPI.Application.Common;

public class ForbiddenException(string message) : Exception(message);
