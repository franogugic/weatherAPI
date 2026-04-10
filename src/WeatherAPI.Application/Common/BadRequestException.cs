namespace WeatherAPI.Application.Common;

public class BadRequestException(string message) : Exception(message);
