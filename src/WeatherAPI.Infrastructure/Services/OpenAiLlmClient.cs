using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Services;

public class OpenAiLlmClient : ILlmClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiLlmClient(HttpClient httpClient, IOptions<OpenAiOptions> options)
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
            throw new InvalidOperationException("OpenAI API key is not configured. Set OpenAI:ApiKey or OpenAI__ApiKey.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new OpenAiResponsesRequest
        {
            Model = _options.Model,
            Instructions = instructions,
            Input = input,
            MaxOutputTokens = _options.MaxOutputTokens,
            Store = false
        }, options: JsonSerializerOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw CreateOpenAiException((int)response.StatusCode, content);

        var parsedResponse = JsonSerializer.Deserialize<OpenAiResponsesResponse>(content, JsonSerializerOptions);
        var outputText = parsedResponse?.OutputText;

        if (!string.IsNullOrWhiteSpace(outputText))
            return outputText.Trim();

        outputText = parsedResponse?.Output?
            .SelectMany(output => output.Content ?? [])
            .Where(contentItem => contentItem.Type == "output_text")
            .Select(contentItem => contentItem.Text)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

        if (!string.IsNullOrWhiteSpace(outputText))
            return outputText.Trim();

        throw new InvalidOperationException("OpenAI response did not contain output text.");
    }

    private static Exception CreateOpenAiException(int statusCode, string content)
    {
        var message = content.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase)
            ? "OpenAI API quota is exhausted or billing is not enabled for this account."
            : "Weather assistant provider is temporarily unavailable.";

        return new ExternalServiceException(message, statusCode);
    }

    private sealed class OpenAiResponsesRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("instructions")]
        public string Instructions { get; init; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; init; } = string.Empty;

        [JsonPropertyName("max_output_tokens")]
        public int MaxOutputTokens { get; init; }

        [JsonPropertyName("store")]
        public bool Store { get; init; }
    }

    private sealed class OpenAiResponsesResponse
    {
        [JsonPropertyName("output_text")]
        public string? OutputText { get; init; }

        [JsonPropertyName("output")]
        public List<OpenAiOutputItem>? Output { get; init; }
    }

    private sealed class OpenAiOutputItem
    {
        [JsonPropertyName("content")]
        public List<OpenAiContentItem>? Content { get; init; }
    }

    private sealed class OpenAiContentItem
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }
}
