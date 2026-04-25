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

        _logger.LogInformation("Sending request to {ProviderName}. Payload size: {PayloadSize} bytes", GetProviderName(), payloadJson.Length);
        _logger.LogDebug("Payload: {Payload}", payloadJson);

        var url = BuildRequestUrl();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        ConfigureRequest(httpRequest, payloadJson);

        var response = await _http.SendAsync(httpRequest);

        _logger.LogInformation("{ProviderName} API response status: {StatusCode} {StatusCodeName}", GetProviderName(), (int)response.StatusCode, response.StatusCode);

        // Handle common error cases
        await HandleErrorResponse(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Response body: {ResponseBody}", responseBody);

        using var doc = JsonDocument.Parse(responseBody);
        var result = ParseResponse(doc);

        _logger.LogInformation("Parsed result - IsToolCall: {IsToolCall}, ToolName: {ToolName}, HasAnswer: {HasAnswer}", 
            result.IsToolCall, result.ToolCall?.Name, !string.IsNullOrEmpty(result.FinalAnswer));

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
            _logger.LogWarning("{ProviderName} API rate limit exceeded. Response: {ErrorBody}", GetProviderName(), errorBody);
            throw new InvalidOperationException(
                "The AI service has reached its request limit. Please try again in a few seconds.");
        }

        // Forbidden
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("{ProviderName} API returned 403 Forbidden. Response: {ErrorBody}", GetProviderName(), errorBody);
            throw new InvalidOperationException(
                $"Access denied to {GetProviderName()} API. Please verify your API key.");
        }

        // Other errors
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("{ProviderName} API error: {ErrorBody}", GetProviderName(), errorBody);
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
