using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherRuleBasedAnswerService
{
    string GenerateAnswer(
        string message,
        ChatWeatherForecastContextDto context,
        string? language);
}
