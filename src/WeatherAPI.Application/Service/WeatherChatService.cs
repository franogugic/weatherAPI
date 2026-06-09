using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<WeatherChatService> _logger;

    public WeatherChatService(
        IWeatherChatRepository weatherChatRepository,
        ILlmClient llmClient,
        IWeatherRuleBasedAnswerService ruleBasedAnswerService,
        ILogger<WeatherChatService> logger)
    {
        _weatherChatRepository = weatherChatRepository;
        _llmClient = llmClient;
        _ruleBasedAnswerService = ruleBasedAnswerService;
        _logger = logger;
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

            if (IsLikelyIncompleteAnswer(answer, request.Message))
            {
                _logger.LogWarning(
                    "Weather chat LLM response looked incomplete for location {LocationId}; using fallback response.",
                    request.LocationId);

                answer = _ruleBasedAnswerService.GenerateAnswer(request.Message, context, request.Language);
                source = "Rules";
            }
        }
        catch (Exception exception) when (ShouldFallbackToRules(exception, cancellationToken))
        {
            _logger.LogWarning(
                exception,
                "Weather chat LLM request failed for location {LocationId}; using fallback response.",
                request.LocationId);

            answer = _ruleBasedAnswerService.GenerateAnswer(request.Message, context, request.Language);
        }

        return new ChatWeatherResponseDto
        {
            Answer = FormatAssistantAnswer(answer, request.Message, request.Language),
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
        var responseLanguage = ResolveResponseLanguage(language);

        return $$"""
            You are a calm, helpful weather assistant inside a weather app.
            Answer in {{responseLanguage}}.
            The selected app language is {{responseLanguage}}. Always use that language, even if the user's message is written in another language.
            The forecast context is the source of truth for weather, location, timing, and measurements.
            Use general meteorological knowledge only to explain what the retrieved values mean or to give practical advice.
            Do not invent missing weather values. If the forecast does not contain enough information for the user's question, say that clearly.

            Style:
            - Sound like a practical local assistant, not a report or a chatbot demo.
            - Be direct, natural, and easy to scan.
            - Do not mention "forecast context", "data", "database", "source of truth", "retrieved values", or internal system wording.
            - Avoid stiff openings like "Based on the provided information" or "According to the data".
            - Return plain text only. Do not use Markdown, bold markers, headings, tables, or code blocks.
            - Prefer one short paragraph for simple questions.
            - Use 3 to 5 short lines only when the user asks for a plan or comparison.
            - Keep most answers under 90 words.

            Weather behavior:
            - Mention the relevant period when the answer depends on time.
            - Give practical advice only when useful, such as umbrella, lighter clothes, water, shade, or wind caution.
            - If conditions are mixed, say what is good and what to watch out for.
            For Croatian answers, format dates as dd.MM.yyyy. HH:mm.
            If the user asks about a named period such as danas, sutra, večeras, today, tomorrow, or tonight, show only the time like 13:00 instead of a full date.
            If the user asks for a plan, return a complete compact plan with short time blocks and do not stop mid-sentence.
            """;
    }

    private static string ResolveResponseLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return "the user's language";

        if (language.StartsWith("hr", StringComparison.OrdinalIgnoreCase))
            return "Croatian (hr-HR)";

        if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            return "English";

        return language;
    }

    private static string FormatAssistantAnswer(string answer, string message, string? language)
    {
        var formattedAnswer = RemoveMarkdownFormatting(answer);
        formattedAnswer = NormalizeWeatherText(formattedAnswer, message, language);

        return FormatAnswerDates(formattedAnswer, message, language);
    }

    private static string RemoveMarkdownFormatting(string answer)
    {
        return answer
            .Replace("**", string.Empty)
            .Replace("__", string.Empty)
            .Replace("```", string.Empty)
            .Replace("`", string.Empty)
            .Trim();
    }

    private static string NormalizeWeatherText(string answer, string message, string? language)
    {
        var normalizedAnswer = Regex.Replace(
            answer,
            @"(?<value>\d+)(?:\.0)?\s*C\b",
            "${value} °C",
            RegexOptions.IgnoreCase);

        if (IsCroatian(message, language))
        {
            normalizedAnswer = Regex.Replace(
                normalizedAnswer,
                @"\bpadalina\b",
                "oborina",
                RegexOptions.IgnoreCase);
            normalizedAnswer = Regex.Replace(
                normalizedAnswer,
                @"\bpadaline\b",
                "oborine",
                RegexOptions.IgnoreCase);
        }

        return normalizedAnswer;
    }

    private static bool IsLikelyIncompleteAnswer(string answer, string message)
    {
        var trimmedAnswer = answer.Trim();

        if (string.IsNullOrWhiteSpace(trimmedAnswer))
            return true;

        var normalizedMessage = Normalize(message);
        var asksForPlan = ContainsAny(normalizedMessage, [
            "plan", "raspored", "aktivnost", "priroda", "izlet", "schedule", "activities", "outdoor"
        ]);

        if (!asksForPlan)
            return false;

        if (trimmedAnswer.EndsWith('.') || trimmedAnswer.EndsWith('!') || trimmedAnswer.EndsWith('?'))
            return false;

        var normalizedAnswer = Normalize(trimmedAnswer);
        return trimmedAnswer.Length < 180
            || normalizedAnswer.EndsWith(" s obzi", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.EndsWith(" s obzirom", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.EndsWith(" with", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.EndsWith(" because", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.EndsWith(" and", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.EndsWith(" i", StringComparison.OrdinalIgnoreCase)
            || normalizedAnswer.EndsWith(" ali", StringComparison.OrdinalIgnoreCase);
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
        builder.AppendLine("Retrieved forecast context:");
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
