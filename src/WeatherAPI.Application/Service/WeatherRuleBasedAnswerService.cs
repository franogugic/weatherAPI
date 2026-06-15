using System.Globalization;
using System.Text;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Application.Service;

public class WeatherRuleBasedAnswerService : IWeatherRuleBasedAnswerService
{
    private static readonly string[] WalkKeywords =
    [
        "setnja", "šetnja", "prosetati", "prošetati", "hodanje", "izaci", "izaći", "vani", "napolje",
        "trcati", "trčati", "trcanje", "trčanje", "jogging", "jogirati",
        "walk", "walking", "stroll", "outside", "go out", "hike", "hiking", "run", "running", "jog"
    ];

    private static readonly string[] RainKeywords =
    [
        "kisa", "kiša", "padati", "pada", "padavine", "oborine", "pljusak", "pljuskovi", "kisobran", "kišobran",
        "rain", "raining", "rainy", "precipitation", "shower", "showers", "umbrella", "wet"
    ];

    private static readonly string[] WindKeywords =
    [
        "vjetar", "vetar", "puse", "puše", "puhati", "bura", "wind", "windy", "gust", "breeze"
    ];

    private static readonly string[] TemperatureKeywords =
    [
        "temperatura", "temp", "hladno", "toplo", "vruce", "vruće", "vrucina", "vrućina", "zagrijati", "ohladiti",
        "temperature", "cold", "warm", "hot", "heat", "cool", "chilly"
    ];

    private static readonly string[] WarmestKeywords =
    [
        "najtopl", "najvru", "toplije", "hottest", "warmest", "warmest time"
    ];

    private static readonly string[] BestTimeKeywords =
    [
        "kada", "kad", "koje vrijeme", "u koliko", "najbolje", "najbolji period",
        "when", "what time", "best time", "best period"
    ];

    private static readonly string[] OutdoorPlanKeywords =
    [
        "plan", "raspored", "aktivnosti", "aktivnost", "priroda", "vani", "napolje", "izlet", "dan",
        "schedule", "activities", "activity", "outdoor", "outside", "nature", "day plan"
    ];

    private static readonly string[] ColdestKeywords =
    [
        "najhlad", "hladnije", "coldest", "coolest", "lowest temperature"
    ];

    private static readonly string[] HumidityKeywords =
    [
        "vlaga", "vlaznost", "vlažnost", "sparno", "humid", "humidity", "muggy"
    ];

    private static readonly string[] CloudKeywords =
    [
        "oblaci", "oblacno", "oblačno", "naoblaka", "cloud", "cloudy", "cloudiness", "overcast"
    ];

    private static readonly string[] PressureKeywords =
    [
        "tlak", "pritisak", "pressure", "hpa", "barometer"
    ];

    private static readonly string[] ClothingKeywords =
    [
        "obuci", "obući", "odjeca", "odjeća", "jakna", "kaput", "majica", "slojevi",
        "wear", "clothes", "clothing", "jacket", "coat", "layers", "outfit"
    ];

    private static readonly string[] TodayKeywords =
    [
        "danas", "today"
    ];

    private static readonly string[] TomorrowKeywords =
    [
        "sutra", "tomorrow"
    ];

    private static readonly string[] TonightKeywords =
    [
        "veceras", "večeras", "nocas", "noćas", "tonight", "night"
    ];

    private static readonly string[] MorningKeywords =
    [
        "jutro", "ujutro", "morning"
    ];

    private static readonly string[] AfternoonKeywords =
    [
        "popodne", "poslijepodne", "afternoon"
    ];

    private static readonly string[] EveningKeywords =
    [
        "večer", "vecer", "evening"
    ];

    public string GenerateAnswer(
        string message,
        ChatWeatherForecastContextDto context,
        string? language)
    {
        var isCroatian = IsCroatian(message, language);
        var normalizedMessage = Normalize(message);
        var forecast = GetRelevantForecast(normalizedMessage, context);

        if (forecast.Count == 0 && context.Current is not null)
            forecast = [context.Current];

        if (forecast.Count == 0)
            return isCroatian
                ? $"Nemam dovoljno prognoze za {context.LocationName} da odgovorim na to pitanje."
                : $"I do not have enough forecast information for {context.LocationName} to answer that question.";

        var stats = ForecastStats.Create(forecast);
        var periodLabel = GetPeriodLabel(normalizedMessage, forecast, isCroatian);
        var detectedIntents = DetectIntents(normalizedMessage);
        stats.SetIntents(detectedIntents);

        if (detectedIntents.Contains(WeatherIntent.OutdoorPlan))
            return BuildOutdoorPlanAnswer(context, forecast, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Walk))
            return BuildWalkAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Warmest))
            return BuildExtremeTemperatureAnswer(context, stats, periodLabel, isCroatian, warmer: true);

        if (detectedIntents.Contains(WeatherIntent.Coldest))
            return BuildExtremeTemperatureAnswer(context, stats, periodLabel, isCroatian, warmer: false);

        if (detectedIntents.Contains(WeatherIntent.Rain))
            return BuildRainAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Wind))
            return BuildWindAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Clothing))
            return BuildClothingAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Humidity))
            return BuildHumidityAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Clouds))
            return BuildCloudAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Pressure))
            return BuildPressureAnswer(context, stats, periodLabel, isCroatian);

        if (detectedIntents.Contains(WeatherIntent.Temperature))
            return BuildTemperatureAnswer(context, stats, periodLabel, isCroatian);

        return BuildOverviewAnswer(context, stats, periodLabel, isCroatian);
    }

    private static List<ChatWeatherForecastItemDto> GetRelevantForecast(
        string normalizedMessage,
        ChatWeatherForecastContextDto context)
    {
        var forecast = context.Upcoming
            .OrderBy(item => item.ForecastTime)
            .ToList();

        if (forecast.Count == 0)
            return [];

        var now = DateTime.UtcNow;
        var today = now.Date;

        if (ContainsAny(normalizedMessage, TomorrowKeywords))
            return forecast
                .Where(item => item.ForecastTime.Date == today.AddDays(1))
                .ToList();

        if (ContainsAny(normalizedMessage, TodayKeywords))
            forecast = forecast
                .Where(item => item.ForecastTime.Date == today)
                .ToList();

        if (ContainsAny(normalizedMessage, TonightKeywords))
            return forecast
                .Where(item => item.ForecastTime.Hour >= 18 || item.ForecastTime.Hour < 6)
                .ToList();

        if (ContainsAny(normalizedMessage, MorningKeywords))
            return forecast
                .Where(item => item.ForecastTime.Hour >= 6 && item.ForecastTime.Hour < 12)
                .ToList();

        if (ContainsAny(normalizedMessage, AfternoonKeywords))
            return forecast
                .Where(item => item.ForecastTime.Hour >= 12 && item.ForecastTime.Hour < 18)
                .ToList();

        if (ContainsAny(normalizedMessage, EveningKeywords))
            return forecast
                .Where(item => item.ForecastTime.Hour >= 18 && item.ForecastTime.Hour < 23)
                .ToList();

        return forecast.Count > 24
            ? forecast.Take(24).ToList()
            : forecast;
    }

    private static HashSet<WeatherIntent> DetectIntents(string normalizedMessage)
    {
        var intents = new HashSet<WeatherIntent>();

        if (ContainsAny(normalizedMessage, WalkKeywords))
            intents.Add(WeatherIntent.Walk);
        if (ContainsAny(normalizedMessage, RainKeywords))
            intents.Add(WeatherIntent.Rain);
        if (ContainsAny(normalizedMessage, WindKeywords))
            intents.Add(WeatherIntent.Wind);
        if (ContainsAny(normalizedMessage, TemperatureKeywords))
            intents.Add(WeatherIntent.Temperature);
        if (ContainsAny(normalizedMessage, WarmestKeywords))
            intents.Add(WeatherIntent.Warmest);
        if (ContainsAny(normalizedMessage, ColdestKeywords))
            intents.Add(WeatherIntent.Coldest);
        if (ContainsAny(normalizedMessage, HumidityKeywords))
            intents.Add(WeatherIntent.Humidity);
        if (ContainsAny(normalizedMessage, CloudKeywords))
            intents.Add(WeatherIntent.Clouds);
        if (ContainsAny(normalizedMessage, PressureKeywords))
            intents.Add(WeatherIntent.Pressure);
        if (ContainsAny(normalizedMessage, ClothingKeywords))
            intents.Add(WeatherIntent.Clothing);
        if (ContainsAny(normalizedMessage, BestTimeKeywords))
            intents.Add(WeatherIntent.BestTime);
        if (ContainsAny(normalizedMessage, OutdoorPlanKeywords))
            intents.Add(WeatherIntent.OutdoorPlan);

        return intents;
    }

    private static string BuildOutdoorPlanAnswer(
        ChatWeatherForecastContextDto context,
        IReadOnlyList<ChatWeatherForecastItemDto> forecast,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        var morning = FindBestItemInWindow(forecast, 6, 12);
        var afternoon = FindBestItemInWindow(forecast, 12, 18);
        var evening = FindBestItemInWindow(forecast, 18, 22);
        var best = FindBestItemInWindow(forecast, 6, 21) ?? stats.BestActivityTime;

        if (isCroatian)
        {
            var builder = new StringBuilder();
            builder.Append($"{periodLabel} za {context.LocationName}: evo laganog plana za aktivnosti u prirodi. ");
            builder.Append($"Najugodniji termin izgleda oko {FormatTimeForPeriod(best.ForecastTime, periodLabel, isCroatian)} ");
            builder.Append($"({FormatTemperature(best.AirTemperature)}, oborine {FormatMillimeters(best.PrecipitationAmount)}, vjetar {FormatWind(best.WindSpeed)}). ");

            if (morning is not null)
                builder.Append($"Ujutro oko {FormatTimeForPeriod(morning.ForecastTime, periodLabel, isCroatian)} dobro je za šetnju ili lagano trčanje. ");
            if (afternoon is not null)
                builder.Append(BuildAfternoonPlanSentence(afternoon, periodLabel, isCroatian));
            if (evening is not null)
                builder.Append($"Navečer oko {FormatTimeForPeriod(evening.ForecastTime, periodLabel, isCroatian)} je dobar termin za mirniju šetnju. ");

            builder.Append(BuildPracticalAdvice(stats, isCroatian));
            return builder.ToString();
        }

        var englishBuilder = new StringBuilder();
        englishBuilder.Append($"{periodLabel} in {context.LocationName}: here is a light outdoor activity plan. ");
        englishBuilder.Append($"The most comfortable time looks around {FormatTimeForPeriod(best.ForecastTime, periodLabel, isCroatian)} ");
        englishBuilder.Append($"({FormatTemperature(best.AirTemperature)}, precipitation {FormatMillimeters(best.PrecipitationAmount)}, wind {FormatWind(best.WindSpeed)}). ");

        if (morning is not null)
            englishBuilder.Append($"In the morning around {FormatTimeForPeriod(morning.ForecastTime, periodLabel, isCroatian)}, a walk or light run looks suitable. ");
        if (afternoon is not null)
            englishBuilder.Append(BuildAfternoonPlanSentence(afternoon, periodLabel, isCroatian));
        if (evening is not null)
            englishBuilder.Append($"In the evening around {FormatTimeForPeriod(evening.ForecastTime, periodLabel, isCroatian)}, a calmer walk looks good. ");

        englishBuilder.Append(BuildPracticalAdvice(stats, isCroatian));
        return englishBuilder.ToString();
    }

    private static string BuildAfternoonPlanSentence(
        ChatWeatherForecastItemDto afternoon,
        string periodLabel,
        bool isCroatian)
    {
        var time = FormatTimeForPeriod(afternoon.ForecastTime, periodLabel, isCroatian);

        if (isCroatian)
        {
            return afternoon.AirTemperature >= 27m
                ? $"Popodne oko {time} biraj kraću aktivnost ili odmor u hladu jer je toplije. "
                : $"Popodne oko {time} možeš planirati kraću šetnju ili laganu aktivnost. ";
        }

        return afternoon.AirTemperature >= 27m
            ? $"In the afternoon around {time}, choose a shorter activity or shade because it is warmer. "
            : $"In the afternoon around {time}, a shorter walk or light activity works well. ";
    }

    private static string BuildWalkAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (stats.Intents.Contains(WeatherIntent.BestTime))
            return BuildBestActivityTimeAnswer(context, stats, periodLabel, isCroatian);

        var score = 0;
        score += stats.MaxPrecipitation <= 0.2m ? 2 : stats.MaxPrecipitation <= 1m ? 1 : -2;
        score += stats.MaxWindSpeed <= 5m ? 2 : stats.MaxWindSpeed <= 9m ? 0 : -2;
        score += stats.AverageTemperature is >= 12m and <= 26m ? 2 : stats.AverageTemperature is >= 7m and <= 30m ? 1 : -1;

        if (isCroatian)
        {
            var verdict = score >= 4
                ? "izgleda dobro za šetnju"
                : score >= 1
                    ? "može proći za šetnju, ali uz malo opreza"
                    : "nije idealno za šetnju";

            return $"{periodLabel} za {context.LocationName} {verdict}. " +
                   $"Temperatura je oko {FormatTemperature(stats.AverageTemperature)}, oborine do {FormatMillimeters(stats.MaxPrecipitation)}, a vjetar do {FormatWind(stats.MaxWindSpeed)}. " +
                   $"{BuildPracticalAdvice(stats, isCroatian)}";
        }

        var englishVerdict = score >= 4
            ? "looks good for a walk"
            : score >= 1
                ? "can work for a walk, but with a little caution"
                : "does not look ideal for a walk";

        return $"{periodLabel} in {context.LocationName} {englishVerdict}. " +
               $"The temperature is around {FormatTemperature(stats.AverageTemperature)}, precipitation up to {FormatMillimeters(stats.MaxPrecipitation)}, and wind up to {FormatWind(stats.MaxWindSpeed)}. " +
               $"{BuildPracticalAdvice(stats, isCroatian)}";
    }

    private static string BuildBestActivityTimeAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        var best = stats.BestActivityTime;
        var time = FormatTimeForPeriod(best.ForecastTime, periodLabel, isCroatian);

        if (isCroatian)
        {
            return $"{periodLabel} za {context.LocationName}, najbolji period za aktivnost izgleda oko {time}. " +
                   $"Tada je temperatura {FormatTemperature(best.AirTemperature)}, oborine {FormatMillimeters(best.PrecipitationAmount)}, a vjetar {FormatWind(best.WindSpeed)}.";
        }

        return $"{periodLabel} in {context.LocationName}, the best time for activity looks around {time}. " +
               $"At that time, temperature is {FormatTemperature(best.AirTemperature)}, precipitation {FormatMillimeters(best.PrecipitationAmount)}, and wind {FormatWind(best.WindSpeed)}.";
    }

    private static string BuildRainAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (isCroatian)
        {
            var rainSummary = stats.MaxPrecipitation <= 0m
                ? "ne očekuju se oborine"
                : stats.MaxPrecipitation <= 0.5m
                    ? "moguće su vrlo slabe oborine"
                    : stats.MaxPrecipitation <= 2m
                        ? "postoji šansa za kišu"
                        : "kiša izgleda dosta vjerovatno";

            return $"{periodLabel} za {context.LocationName}: {rainSummary}. " +
                   $"Najveća količina oborine po satu je {FormatMillimeters(stats.MaxPrecipitation)}, a ukupno u periodu oko {FormatMillimeters(stats.TotalPrecipitation)}. " +
                   $"{(stats.MaxPrecipitation > 0.5m ? "Ponesi kišobran ili jaknu za kišu." : "Kišobran vjerovatno nije potreban, ali provjeri opet ako ideš kasnije.")}";
        }

        var summary = stats.MaxPrecipitation <= 0m
            ? "precipitation is not expected"
            : stats.MaxPrecipitation <= 0.5m
                ? "very light precipitation is possible"
                : stats.MaxPrecipitation <= 2m
                    ? "there is a chance of rain"
                    : "rain looks fairly likely";

        return $"{periodLabel} in {context.LocationName}: {summary}. " +
               $"The highest hourly precipitation is {FormatMillimeters(stats.MaxPrecipitation)}, with about {FormatMillimeters(stats.TotalPrecipitation)} total in the period. " +
               $"{(stats.MaxPrecipitation > 0.5m ? "Bring an umbrella or a rain jacket." : "An umbrella probably is not necessary, but check again if you go later.")}";
    }

    private static string BuildWindAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (isCroatian)
        {
            var description = stats.MaxWindSpeed <= 3m
                ? "vjetar je slab"
                : stats.MaxWindSpeed <= 8m
                    ? "vjetar je umjeren"
                    : "vjetar može biti jak i neugodan";

            return $"{periodLabel} za {context.LocationName}: {description}. " +
                   $"Prosjek je {FormatWind(stats.AverageWindSpeed)}, a maksimum {FormatWind(stats.MaxWindSpeed)}. " +
                   $"{(stats.MaxWindSpeed > 8m ? "Ako ideš vani, računaj na hladniji subjektivni osjećaj i jače nalete." : "Vjetar ne bi trebao biti glavni problem.")}";
        }

        var summary = stats.MaxWindSpeed <= 3m
            ? "wind is light"
            : stats.MaxWindSpeed <= 8m
                ? "wind is moderate"
                : "wind may feel strong and uncomfortable";

        return $"{periodLabel} in {context.LocationName}: {summary}. " +
               $"Average wind is {FormatWind(stats.AverageWindSpeed)}, with a maximum of {FormatWind(stats.MaxWindSpeed)}. " +
               $"{(stats.MaxWindSpeed > 8m ? "If you go outside, expect it to feel cooler and gustier." : "Wind should not be the main issue.")}";
    }

    private static string BuildTemperatureAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        return isCroatian
            ? $"{periodLabel} za {context.LocationName}: temperatura se kreće od {FormatTemperature(stats.MinTemperature)} do {FormatTemperature(stats.MaxTemperature)}, prosječno oko {FormatTemperature(stats.AverageTemperature)}."
            : $"{periodLabel} in {context.LocationName}: temperature ranges from {FormatTemperature(stats.MinTemperature)} to {FormatTemperature(stats.MaxTemperature)}, averaging around {FormatTemperature(stats.AverageTemperature)}.";
    }

    private static string BuildExtremeTemperatureAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian,
        bool warmer)
    {
        var item = warmer ? stats.Warmest : stats.Coldest;
        var time = FormatTimeForPeriod(item.ForecastTime, periodLabel, isCroatian);

        if (isCroatian)
        {
            var label = warmer ? "najtoplije" : "najhladnije";
            return $"{periodLabel} za {context.LocationName}, {label} izgleda oko {time}, sa temperaturom {FormatTemperature(item.AirTemperature)}.";
        }

        var englishLabel = warmer ? "warmest" : "coldest";
        return $"{periodLabel} in {context.LocationName}, the {englishLabel} point looks around {time}, with a temperature of {FormatTemperature(item.AirTemperature)}.";
    }

    private static string BuildHumidityAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (isCroatian)
        {
            var description = stats.AverageHumidity >= 80m
                ? "vlaga je visoka, pa se vrijeme može osjećati teže ili sparnije"
                : stats.AverageHumidity >= 60m
                    ? "vlaga je umjerena do povišena"
                    : "vlaga nije posebno visoka";

            return $"{periodLabel} za {context.LocationName}: {description}. Prosječna vlaga je oko {FormatPercent(stats.AverageHumidity)}.";
        }

        var summary = stats.AverageHumidity >= 80m
            ? "humidity is high, so the weather may feel heavier or muggy"
            : stats.AverageHumidity >= 60m
                ? "humidity is moderate to elevated"
                : "humidity is not especially high";

        return $"{periodLabel} in {context.LocationName}: {summary}. Average humidity is around {FormatPercent(stats.AverageHumidity)}.";
    }

    private static string BuildCloudAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (isCroatian)
        {
            var description = stats.AverageCloudiness >= 75m
                ? "pretežno oblačno"
                : stats.AverageCloudiness >= 40m
                    ? "djelomično oblačno"
                    : "uglavnom vedrije";

            return $"{periodLabel} za {context.LocationName}: izgleda {description}. Prosječna naoblaka je oko {FormatPercent(stats.AverageCloudiness)}.";
        }

        var summary = stats.AverageCloudiness >= 75m
            ? "mostly cloudy"
            : stats.AverageCloudiness >= 40m
                ? "partly cloudy"
                : "mostly clearer";

        return $"{periodLabel} in {context.LocationName}: it looks {summary}. Average cloudiness is around {FormatPercent(stats.AverageCloudiness)}.";
    }

    private static string BuildPressureAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        return isCroatian
            ? $"{periodLabel} za {context.LocationName}: tlak zraka je oko {FormatPressure(stats.AveragePressure)}. Veće promjene tlaka mogu utjecati na osjećaj vremena, ali za detaljnije zaključke treba pratiti trend kroz duži period."
            : $"{periodLabel} in {context.LocationName}: air pressure is around {FormatPressure(stats.AveragePressure)}. Bigger pressure changes can affect how the weather feels, but a longer trend is needed for stronger conclusions.";
    }

    private static string BuildClothingAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (isCroatian)
        {
            var clothing = stats.AverageTemperature < 8m
                ? "obuci topliju jaknu"
                : stats.AverageTemperature < 16m
                    ? "lagana jakna ili slojevi su dobra ideja"
                    : stats.AverageTemperature < 26m
                        ? "laganija odjeća bi trebala biti dovoljna"
                        : "biraj laganu odjeću i ponesi vodu";

            var rain = stats.MaxPrecipitation > 0.5m ? " Ponesi i zaštitu od kiše." : "";
            return $"{periodLabel} za {context.LocationName}: {clothing}. Temperatura je oko {FormatTemperature(stats.AverageTemperature)}, vjetar do {FormatWind(stats.MaxWindSpeed)}.{rain}";
        }

        var suggestion = stats.AverageTemperature < 8m
            ? "wear a warm jacket"
            : stats.AverageTemperature < 16m
                ? "a light jacket or layers are a good idea"
                : stats.AverageTemperature < 26m
                    ? "lighter clothing should be enough"
                    : "choose light clothing and bring water";

        var rainNote = stats.MaxPrecipitation > 0.5m ? " Bring rain protection too." : "";
        return $"{periodLabel} in {context.LocationName}: {suggestion}. Temperature is around {FormatTemperature(stats.AverageTemperature)}, with wind up to {FormatWind(stats.MaxWindSpeed)}.{rainNote}";
    }

    private static string BuildOverviewAnswer(
        ChatWeatherForecastContextDto context,
        ForecastStats stats,
        string periodLabel,
        bool isCroatian)
    {
        if (isCroatian)
        {
            return $"{periodLabel} za {context.LocationName}: temperatura je od {FormatTemperature(stats.MinTemperature)} do {FormatTemperature(stats.MaxTemperature)}, " +
                   $"oborine do {FormatMillimeters(stats.MaxPrecipitation)}, vjetar do {FormatWind(stats.MaxWindSpeed)}, vlaga oko {FormatPercent(stats.AverageHumidity)}. " +
                   $"{BuildPracticalAdvice(stats, isCroatian)}";
        }

        return $"{periodLabel} in {context.LocationName}: temperature ranges from {FormatTemperature(stats.MinTemperature)} to {FormatTemperature(stats.MaxTemperature)}, " +
               $"precipitation up to {FormatMillimeters(stats.MaxPrecipitation)}, wind up to {FormatWind(stats.MaxWindSpeed)}, humidity around {FormatPercent(stats.AverageHumidity)}. " +
               $"{BuildPracticalAdvice(stats, isCroatian)}";
    }

    private static string BuildPracticalAdvice(ForecastStats stats, bool isCroatian)
    {
        var notes = new List<string>();

        if (stats.MaxPrecipitation > 0.5m)
            notes.Add(isCroatian ? "ponesi kišobran" : "bring an umbrella");
        if (stats.MaxWindSpeed > 8m)
            notes.Add(isCroatian ? "računaj na jači vjetar" : "expect stronger wind");
        if (stats.AverageTemperature < 8m)
            notes.Add(isCroatian ? "obuci se toplije" : "dress warmly");
        if (stats.AverageTemperature > 28m)
            notes.Add(isCroatian ? "ponesi vodu i izbjegni najtopliji dio dana" : "bring water and avoid the hottest part of the day");

        if (notes.Count == 0)
            return isCroatian
                ? "Uvjeti izgledaju uglavnom stabilno."
                : "Conditions look mostly stable.";

        return isCroatian
            ? $"Savjet: {string.Join(", ", notes)}."
            : $"Tip: {string.Join(", ", notes)}.";
    }

    private static ChatWeatherForecastItemDto? FindBestItemInWindow(
        IReadOnlyList<ChatWeatherForecastItemDto> forecast,
        int startHour,
        int endHour)
    {
        return forecast
            .Where(item => item.ForecastTime.Hour >= startHour && item.ForecastTime.Hour < endHour)
            .OrderByDescending(ForecastStats.GetActivityScore)
            .ThenBy(item => item.ForecastTime)
            .FirstOrDefault();
    }

    private static string GetPeriodLabel(
        string normalizedMessage,
        IReadOnlyCollection<ChatWeatherForecastItemDto> forecast,
        bool isCroatian)
    {
        if (ContainsAny(normalizedMessage, TomorrowKeywords))
            return isCroatian ? "Sutra" : "Tomorrow";
        if (ContainsAny(normalizedMessage, TonightKeywords))
            return isCroatian ? "Večeras" : "Tonight";
        if (ContainsAny(normalizedMessage, MorningKeywords))
            return isCroatian ? "Ujutro" : "In the morning";
        if (ContainsAny(normalizedMessage, AfternoonKeywords))
            return isCroatian ? "Popodne" : "In the afternoon";
        if (ContainsAny(normalizedMessage, EveningKeywords))
            return isCroatian ? "Navečer" : "In the evening";
        if (ContainsAny(normalizedMessage, TodayKeywords))
            return isCroatian ? "Danas" : "Today";

        var first = forecast.Min(item => item.ForecastTime);
        var last = forecast.Max(item => item.ForecastTime);

        return isCroatian
            ? $"Prema podacima od {FormatDateTime(first, isCroatian)} do {FormatDateTime(last, isCroatian)}"
            : $"Based on data from {FormatDateTime(first, isCroatian)} to {FormatDateTime(last, isCroatian)}";
    }

    private static bool IsCroatian(string message, string? language)
    {
        if (!string.IsNullOrWhiteSpace(language)
            && language.StartsWith("hr", StringComparison.OrdinalIgnoreCase))
            return true;

        var normalized = Normalize(message);
        return ContainsAny(normalized, [
            "danas", "sutra", "vrijeme", "vreme", "kisa", "setnja", "vjetar", "temperatura",
            "oblacno", "vlaga", "obuci", "kisobran", "prognoza"
        ]);
    }

    private static bool ContainsAny(string value, IEnumerable<string> keywords)
    {
        return keywords.Any(keyword => value.Contains(Normalize(keyword), StringComparison.OrdinalIgnoreCase));
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

    private static string FormatTemperature(decimal? value)
    {
        return value.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(value.Value, 1)} °C")
            : "unknown";
    }

    private static string FormatMillimeters(decimal? value)
    {
        return value.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(value.Value, 1)} mm")
            : "unknown";
    }

    private static string FormatWind(decimal? value)
    {
        return value.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(value.Value, 1)} m/s")
            : "unknown";
    }

    private static string FormatPercent(decimal? value)
    {
        return value.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(value.Value, 0)}%")
            : "unknown";
    }

    private static string FormatPressure(decimal? value)
    {
        return value.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(value.Value, 0)} hPa")
            : "unknown";
    }

    private static string FormatTimeForPeriod(DateTime value, string periodLabel, bool isCroatian)
    {
        return IsNamedPeriod(periodLabel)
            ? value.ToString("HH:mm", CultureInfo.InvariantCulture)
            : FormatDateTime(value, isCroatian);
    }

    private static string FormatDateTime(DateTime value, bool isCroatian)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString(
            isCroatian ? "dd.MM.yyyy. HH:mm" : "yyyy-MM-dd HH:mm",
            CultureInfo.InvariantCulture);
    }

    private static bool IsNamedPeriod(string periodLabel)
    {
        return periodLabel is
            "Danas" or
            "Sutra" or
            "Večeras" or
            "Ujutro" or
            "Popodne" or
            "Navečer" or
            "Today" or
            "Tomorrow" or
            "Tonight" or
            "In the morning" or
            "In the afternoon" or
            "In the evening";
    }

    private enum WeatherIntent
    {
        Walk,
        Rain,
        Wind,
        Temperature,
        Warmest,
        Coldest,
        Humidity,
        Clouds,
        Pressure,
        Clothing,
        BestTime,
        OutdoorPlan
    }

    private sealed class ForecastStats
    {
        private ForecastStats(IReadOnlyList<ChatWeatherForecastItemDto> items)
        {
            Warmest = items
                .Where(item => item.AirTemperature.HasValue)
                .OrderByDescending(item => item.AirTemperature)
                .FirstOrDefault() ?? items[0];
            Coldest = items
                .Where(item => item.AirTemperature.HasValue)
                .OrderBy(item => item.AirTemperature)
                .FirstOrDefault() ?? items[0];
            AverageTemperature = Average(items.Select(item => item.AirTemperature));
            MinTemperature = items.Select(item => item.AirTemperature).Where(value => value.HasValue).Min();
            MaxTemperature = items.Select(item => item.AirTemperature).Where(value => value.HasValue).Max();
            MaxPrecipitation = items.Select(item => item.PrecipitationAmount).Where(value => value.HasValue).DefaultIfEmpty(0).Max();
            TotalPrecipitation = items.Select(item => item.PrecipitationAmount).Where(value => value.HasValue).DefaultIfEmpty(0).Sum();
            AverageWindSpeed = Average(items.Select(item => item.WindSpeed));
            MaxWindSpeed = items.Select(item => item.WindSpeed).Where(value => value.HasValue).DefaultIfEmpty(0).Max();
            AverageHumidity = Average(items.Select(item => item.Humidity.HasValue ? (decimal?)item.Humidity.Value : null));
            AverageCloudiness = Average(items.Select(item => item.Cloudiness.HasValue ? (decimal?)item.Cloudiness.Value : null));
            AveragePressure = Average(items.Select(item => item.AirPressureAtSeaLevel));
            BestActivityTime = items
                .OrderByDescending(GetActivityScore)
                .ThenBy(item => item.ForecastTime)
                .FirstOrDefault() ?? items[0];
        }

        public ChatWeatherForecastItemDto Warmest { get; }
        public ChatWeatherForecastItemDto Coldest { get; }
        public decimal? AverageTemperature { get; }
        public decimal? MinTemperature { get; }
        public decimal? MaxTemperature { get; }
        public decimal? MaxPrecipitation { get; }
        public decimal? TotalPrecipitation { get; }
        public decimal? AverageWindSpeed { get; }
        public decimal? MaxWindSpeed { get; }
        public decimal? AverageHumidity { get; }
        public decimal? AverageCloudiness { get; }
        public decimal? AveragePressure { get; }
        public ChatWeatherForecastItemDto BestActivityTime { get; }
        public HashSet<WeatherIntent> Intents { get; private set; } = [];

        public static ForecastStats Create(IReadOnlyList<ChatWeatherForecastItemDto> items)
        {
            return new ForecastStats(items);
        }

        public void SetIntents(HashSet<WeatherIntent> intents)
        {
            Intents = intents;
        }

        public static decimal GetActivityScore(ChatWeatherForecastItemDto item)
        {
            var temperature = item.AirTemperature ?? 18m;
            var precipitation = item.PrecipitationAmount ?? 0m;
            var wind = item.WindSpeed ?? 0m;
            var humidity = item.Humidity ?? 50;

            var temperatureScore = 30m - Math.Abs(temperature - 18m);
            var rainPenalty = precipitation * 12m;
            var windPenalty = wind > 4m ? (wind - 4m) * 2m : 0m;
            var humidityPenalty = humidity > 75 ? (humidity - 75) * 0.2m : 0m;

            return temperatureScore - rainPenalty - windPenalty - humidityPenalty;
        }

        private static decimal? Average(IEnumerable<decimal?> values)
        {
            var valueList = values
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            return valueList.Count == 0
                ? null
                : valueList.Average();
        }
    }
}
