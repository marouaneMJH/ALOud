using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ALOud.Services.Rag;

[Obsolete("Use GeminiLLMClient instead. Grok API integration is deprecated.")]
public sealed class GrokLLMClient : IRagLLMClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    private readonly ILogger<GrokLLMClient> _logger;
    public GrokLLMClient(HttpClient http, ILogger<GrokLLMClient> logger)
    {
        _http = http;
        _apiKey = Environment.GetEnvironmentVariable("GROK_API_KEY")
            ?? throw new InvalidCastException("GROK_API_KEY missing");
        _logger = logger;
    }
    public async Task<RagLLMResult> ExecuteAsync(RagLLMRequest request)
    {
        var payload = BuildPayload(request);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.x.ai/v1/chat/completions"
        );


        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
        Encoding.UTF8,
        "application/json"
        );

        var response = await _http.SendAsync(httpRequest);

        // Log response details for debugging
        _logger.LogInformation($"Grok API response status: {(int)response.StatusCode} {response.StatusCode}");

        // Handle rate limiting
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Grok API rate limit exceeded");
            throw new InvalidOperationException(
                "Le service d'IA a atteint sa limite de requêtes. Veuillez réessayer dans quelques secondes.");
        }

        // Handle forbidden
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Grok API returned 403 Forbidden. Response: {errorBody}");
            throw new InvalidOperationException(
                "Accès refusé à l'API Grok. Vérifiez votre clé API.");
        }

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        return ParseResponse(doc);

    }


    private static object BuildPayload(RagLLMRequest request)
    {
        var messages = new List<object>
    {
        new
        {
            role = "system",
            content = request.SystemPrompt
        },
        new
        {
            role = "user",
            content = BuildUserContent(request)
        }
    };

        return new
        {
            model = "grok-beta",
            temperature = 0.1,
            messages,
            tools = request.Tools?.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.ParametersSchema
                }
            }),
            tool_choice = "auto"
        };
    }

    private static string BuildUserContent(RagLLMRequest request)
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



    private static RagLLMResult ParseResponse(JsonDocument doc)
    {
        var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

        if (message.TryGetProperty("tool_calls", out var toolCalls))
        {
            var call = toolCalls[0];
            var argumentsJson = call.GetProperty("function").GetProperty("arguments").GetString()!;

            return new RagLLMResult
            {
                ToolCall = new RagToolCall
                {
                    Name = call.GetProperty("function").GetProperty("name").GetString()!,
                    Arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson) ?? new()
                }
            };
        }

        return new RagLLMResult
        {
            FinalAnswer = message.GetProperty("content").GetString()
        };
    }
}