using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Services;

public class GeminiLlmClient : ILlmClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiLlmClient(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateAsync(
        string instructions,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Gemini API key is not configured. Set Gemini:ApiKey or Gemini__ApiKey.");

        var requestBody = new GeminiGenerateContentRequest
        {
            SystemInstruction = new GeminiContent
            {
                Parts =
                [
                    new GeminiPart { Text = instructions }
                ]
            },
            Contents =
            [
                new GeminiContent
                {
                    Role = "user",
                    Parts =
                    [
                        new GeminiPart { Text = input }
                    ]
                }
            ],
            GenerationConfig = new GeminiGenerationConfig
            {
                MaxOutputTokens = _options.MaxOutputTokens,
                Temperature = _options.Temperature
            }
        };

        var maxAttempts = _options.MaxRetryAttempts + 1;
        HttpResponseMessage? response = null;
        string content = string.Empty;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var request = CreateRequest(requestBody);
            response = await _httpClient.SendAsync(request, cancellationToken);
            content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode || !ShouldRetry((int)response.StatusCode) || attempt == maxAttempts)
                break;

            response.Dispose();
            var delay = TimeSpan.FromMilliseconds(_options.RetryDelayMilliseconds * attempt);
            await Task.Delay(delay, cancellationToken);
        }

        if (response is null)
            throw new InvalidOperationException("Gemini request was not sent.");

        if (!response.IsSuccessStatusCode)
            throw CreateGeminiException((int)response.StatusCode, content);

        var parsedResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(
            content,
            JsonSerializerOptions);

        if (parsedResponse?.Candidates?.Any(candidate =>
                candidate.FinishReason?.Equals("MAX_TOKENS", StringComparison.OrdinalIgnoreCase) == true) == true)
            throw new ExternalServiceException("Gemini response was truncated by the output token limit.", 502);

        var outputText = string.Join(
            Environment.NewLine,
            parsedResponse?.Candidates?
            .SelectMany(candidate => candidate.Content?.Parts ?? [])
            .Select(part => part.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text)) ?? []);

        if (!string.IsNullOrWhiteSpace(outputText))
            return outputText.Trim();

        throw new InvalidOperationException("Gemini response did not contain output text.");
    }

    private HttpRequestMessage CreateRequest(GeminiGenerateContentRequest requestBody)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"v1beta/models/{_options.Model}:generateContent");
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = JsonContent.Create(requestBody, options: JsonSerializerOptions);

        return request;
    }

    private static bool ShouldRetry(int statusCode)
    {
        return statusCode is 408 or 429 or 500 or 502 or 503 or 504;
    }

    private static Exception CreateGeminiException(int statusCode, string content)
    {
        var providerMessage = ExtractProviderErrorMessage(content);
        var message = statusCode switch
        {
            401 or 403 =>
                "Gemini API key is invalid or does not have access.",
            429 =>
                "Gemini free tier limit was reached.",
            _ => "Gemini request failed."
        };

        return new ExternalServiceException(
            content.Contains("API key not valid", StringComparison.OrdinalIgnoreCase)
                ? "Gemini API key is invalid."
                : $"{message} Status: {statusCode}. Details: {providerMessage}",
            statusCode);
    }

    private static string ExtractProviderErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "No response body.";

        try
        {
            var errorResponse = JsonSerializer.Deserialize<GeminiErrorResponse>(
                content,
                JsonSerializerOptions);

            if (!string.IsNullOrWhiteSpace(errorResponse?.Error?.Message))
                return errorResponse.Error.Message;
        }
        catch (JsonException)
        {
        }

        return content.Length <= 500
            ? content
            : string.Concat(content.AsSpan(0, 500), "...");
    }

    private sealed class GeminiGenerateContentRequest
    {
        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; init; }

        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; init; } = [];

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; init; }
    }

    private sealed class GeminiErrorResponse
    {
        [JsonPropertyName("error")]
        public GeminiError? Error { get; init; }
    }

    private sealed class GeminiError
    {
        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; init; }

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; init; } = [];
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; init; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }
    }

    private sealed class GeminiGenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; init; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; init; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; init; }
    }
}
