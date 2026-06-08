using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Application.Service;

public class WeatherChatService : IWeatherChatService
{
    private const int ForecastDays = 3;
    private readonly IWeatherChatRepository _weatherChatRepository;
    private readonly ILlmClient _llmClient;
    private readonly IWeatherRuleBasedAnswerService _ruleBasedAnswerService;

    public WeatherChatService(
        IWeatherChatRepository weatherChatRepository,
        ILlmClient llmClient,
        IWeatherRuleBasedAnswerService ruleBasedAnswerService)
    {
        _weatherChatRepository = weatherChatRepository;
        _llmClient = llmClient;
        _ruleBasedAnswerService = ruleBasedAnswerService;
    }

    public async Task<ChatWeatherResponseDto> AskAsync(
        ChatWeatherRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var context = await _weatherChatRepository.GetForecastContextAsync(
            request.LocationId,
            ForecastDays,
            cancellationToken);

        if (context is null)
            throw new NotFoundException($"Weather data for location ID {request.LocationId} was not found.");

        var answer = string.Empty;
        var source = "Rules";

        try
        {
            var instructions = BuildInstructions(request.Language);
            var input = BuildInput(request.Message, context);
            answer = await _llmClient.GenerateAsync(instructions, input, cancellationToken);
            source = "LLM";
        }
        catch (Exception exception) when (ShouldFallbackToRules(exception, cancellationToken))
        {
            answer = _ruleBasedAnswerService.GenerateAnswer(request.Message, context, request.Language);
        }

        return new ChatWeatherResponseDto
        {
            Answer = FormatAnswerDates(answer, request.Message, request.Language),
            LocationName = context.LocationName,
            DataUpdatedAt = context.UpdatedAt.HasValue
                ? DateTime.SpecifyKind(context.UpdatedAt.Value, DateTimeKind.Utc)
                : null,
            Source = source
        };
    }

    private static bool ShouldFallbackToRules(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
            return false;

        return exception is ExternalServiceException
            or InvalidOperationException
            or HttpRequestException
            or TaskCanceledException;
    }

    private static string BuildInstructions(string? language)
    {
        var responseLanguage = string.IsNullOrWhiteSpace(language)
            ? "the user's language"
            : language;

        return $$"""
            You are a weather assistant inside a weather application.
            Answer in {{responseLanguage}}.
            The database context is the primary source of truth for weather, location, timing, and measurements.
            Use general meteorological knowledge only to explain what the retrieved values mean or to give practical advice.
            Do not invent missing weather values. If the database does not contain enough data for the user's question, say that clearly.
            Mention the relevant period when the answer depends on time.
            For Croatian answers, format dates as dd.MM.yyyy. HH:mm.
            If the user asks about a named period such as danas, sutra, večeras, today, tomorrow, or tonight, show only the time like 13:00 instead of a full date.
            Keep the answer concise, practical, and friendly.
            """;
    }

    private static string FormatAnswerDates(string answer, string message, string? language)
    {
        var isCroatian = IsCroatian(message, language);
        var normalizedMessage = Normalize(message);
        var shouldUseShortTimes = ContainsAny(normalizedMessage, [
            "danas", "sutra", "veceras", "nocas", "ujutro", "jutro", "popodne", "poslijepodne",
            "navecer", "today", "tomorrow", "tonight", "morning", "afternoon", "evening"
        ]);

        if (shouldUseShortTimes)
            return Regex.Replace(answer, @"\b\d{4}-\d{2}-\d{2}\s+(\d{2}:\d{2})\b", "$1");

        if (!isCroatian)
            return answer;

        return Regex.Replace(
            answer,
            @"\b(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})\s+(?<time>\d{2}:\d{2})\b",
            "${day}.${month}.${year}. ${time}");
    }

    private static bool IsCroatian(string message, string? language)
    {
        if (!string.IsNullOrWhiteSpace(language)
            && language.StartsWith("hr", StringComparison.OrdinalIgnoreCase))
            return true;

        var normalizedMessage = Normalize(message);
        return ContainsAny(normalizedMessage, [
            "danas", "sutra", "vrijeme", "vreme", "kisa", "setnja", "trcati", "vjetar",
            "temperatura", "oblacno", "vlaga", "obuci", "kisobran", "prognoza"
        ]);
    }

    private static bool ContainsAny(string value, IEnumerable<string> keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string value)
    {
        var normalized = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    private static string BuildInput(string message, ChatWeatherForecastContextDto context)
    {
        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Current UTC time: {DateTime.UtcNow:yyyy-MM-dd HH:mm}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"User question: {message}");
        builder.AppendLine();
        builder.AppendLine("Retrieved database context:");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Location: {context.LocationName} (ID {context.LocationId})");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Coordinates: {context.Latitude}, {context.Longitude}, altitude: {FormatValue(context.Altitude, "m")}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Forecast fetched at UTC: {context.FetchedAt:yyyy-MM-dd HH:mm}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Forecast updated at UTC: {FormatDate(context.UpdatedAt)}");
        builder.AppendLine();

        builder.AppendLine("Current/nearest known forecast:");
        builder.AppendLine(context.Current is null
            ? "- No current forecast item was found."
            : FormatForecastItem(context.Current));

        builder.AppendLine();
        builder.AppendLine($"Upcoming hourly forecast rows ({context.Upcoming.Count}):");

        foreach (var item in context.Upcoming)
            builder.AppendLine(FormatForecastItem(item));

        return builder.ToString();
    }

    private static string FormatForecastItem(ChatWeatherForecastItemDto item)
    {
        return string.Create(CultureInfo.InvariantCulture, $"- {item.ForecastTime:yyyy-MM-dd HH:mm} UTC: temp {FormatValue(item.AirTemperature, "C")}, precipitation {FormatValue(item.PrecipitationAmount, "mm")}, wind {FormatValue(item.WindSpeed, "m/s")} from {FormatValue(item.WindDirection, "deg")}, humidity {FormatValue(item.Humidity, "%")}, cloudiness {FormatValue(item.Cloudiness, "%")}, pressure {FormatValue(item.AirPressureAtSeaLevel, "hPa")}, symbol {item.WeatherSymbol ?? "unknown"}");
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            : "unknown";
    }

    private static string FormatValue<T>(T? value, string unit)
        where T : struct
    {
        return value.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"{value.Value} {unit}")
            : "unknown";
    }
}
