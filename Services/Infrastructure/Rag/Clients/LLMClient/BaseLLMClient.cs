using System.Net;
using System.Text;
using System.Text.Json;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Rag.Clients;

/// <summary>
/// Base abstract class for all LLM clients providing common functionality
/// </summary>
public abstract class BaseLLMClient : IRagLLMClient
{
    protected readonly HttpClient _http;
    protected readonly string _apiKey;
    protected readonly ILogger _logger;

    protected BaseLLMClient(HttpClient http, ILogger logger, string apiKeyEnvVar)
    {
        _http = http;
        _apiKey = Environment.GetEnvironmentVariable(apiKeyEnvVar)
            ?? throw new InvalidOperationException($"{apiKeyEnvVar} environment variable is missing");
        _logger = logger;
    }

    public async Task<RagLLMResult> ExecuteAsync(RagLLMRequest request)
    {
        var payload = BuildPayload(request);
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });

        _logger.LogInformation($"Sending request to {GetProviderName()}. Payload size: {payloadJson.Length} bytes");
        _logger.LogDebug($"Payload: {payloadJson}");

        var url = BuildRequestUrl();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        ConfigureRequest(httpRequest, payloadJson);

        var response = await _http.SendAsync(httpRequest);

        _logger.LogInformation($"{GetProviderName()} API response status: {(int)response.StatusCode} {response.StatusCode}");

        // Handle common error cases
        await HandleErrorResponse(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogDebug($"Response body: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        var result = ParseResponse(doc);

        _logger.LogInformation($"Parsed result - IsToolCall: {result.IsToolCall}, ToolName: {result.ToolCall?.Name}, HasAnswer: {!string.IsNullOrEmpty(result.FinalAnswer)}");

        return result;
    }

    /// <summary>
    /// Get the provider name for logging
    /// </summary>
    protected abstract string GetProviderName();

    /// <summary>
    /// Build the complete request URL including API key if needed
    /// </summary>
    protected abstract string BuildRequestUrl();

    /// <summary>
    /// Build the payload specific to the provider
    /// </summary>
    protected abstract object BuildPayload(RagLLMRequest request);

    /// <summary>
    /// Configure HTTP request headers and content
    /// </summary>
    protected virtual void ConfigureRequest(HttpRequestMessage request, string payloadJson)
    {
        request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Parse provider-specific response
    /// </summary>
    protected abstract RagLLMResult ParseResponse(JsonDocument doc);

    /// <summary>
    /// Handle error responses with provider-specific logic
    /// </summary>
    protected virtual async Task HandleErrorResponse(HttpResponseMessage response)
    {
        // Rate limiting
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"{GetProviderName()} API rate limit exceeded. Response: {errorBody}");
            throw new InvalidOperationException(
                "The AI service has reached its request limit. Please try again in a few seconds.");
        }

        // Forbidden
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError($"{GetProviderName()} API returned 403 Forbidden. Response: {errorBody}");
            throw new InvalidOperationException(
                $"Access denied to {GetProviderName()} API. Please verify your API key.");
        }

        // Other errors
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError($"{GetProviderName()} API error: {errorBody}");
            response.EnsureSuccessStatusCode();
        }
    }

    /// <summary>
    /// Build user content with optional cart context
    /// </summary>
    protected string BuildUserContent(RagLLMRequest request)
    {
        if (request.Context == null)
            return request.UserMessage;

        return $"""
        USER MESSAGE:
        {request.UserMessage}

        CURRENT CART CONTEXT (JSON):
        {JsonSerializer.Serialize(request.Context, new JsonSerializerOptions
        {
            WriteIndented = true
        })}
        """;
    }
}
