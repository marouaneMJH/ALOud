using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class GeminiLLMClient : IRagLLMClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<GeminiLLMClient> _logger;

    public GeminiLLMClient(HttpClient http, ILogger<GeminiLLMClient> logger)
    {
        _http = http;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("GEMINI_API_KEY missing");
        _logger = logger;
    }

    public async Task<RagLLMResult> ExecuteAsync(RagLLMRequest request)
    {
        var payload = BuildPayload(request);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(httpRequest);

        _logger.LogInformation($"Gemini API response status: {(int)response.StatusCode} {response.StatusCode}");

        // Handle rate limiting
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Gemini API rate limit exceeded");
            throw new InvalidOperationException(
                "Le service d'IA a atteint sa limite de requêtes. Veuillez réessayer dans quelques secondes.");
        }

        // Handle forbidden
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Gemini API returned 403 Forbidden. Response: {errorBody}");
            throw new InvalidOperationException(
                "Accès refusé à l'API Gemini. Vérifiez votre clé API.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Gemini API error: {errorBody}");
            response.EnsureSuccessStatusCode();
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        return ParseResponse(doc);
    }

    private static object BuildPayload(RagLLMRequest request)
    {
        var systemInstruction = new
        {
            parts = new[] { new { text = request.SystemPrompt } }
        };

        var userContent = BuildUserContent(request);

        var contents = new[]
        {
            new
            {
                role = "user",
                parts = new[] { new { text = userContent } }
            }
        };

        var payload = new
        {
            systemInstruction,
            contents,
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 2048
            }
        };

        // Add tools if provided
        if (request.Tools != null && request.Tools.Any())
        {
            var tools = new[]
            {
                new
                {
                    functionDeclarations = request.Tools.Select(t => new
                    {
                        name = t.Name,
                        description = t.Description,
                        parameters = t.ParametersSchema
                    }).ToArray()
                }
            };

            return new
            {
                systemInstruction,
                contents,
                tools,
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 2048
                }
            };
        }

        return payload;
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
        var candidates = doc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("No candidates returned from Gemini API");
        }

        var candidate = candidates[0];
        var content = candidate.GetProperty("content");
        var parts = content.GetProperty("parts");

        if (parts.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("No parts in response");
        }

        var firstPart = parts[0];

        // Check if it's a function call
        if (firstPart.TryGetProperty("functionCall", out var functionCall))
        {
            var name = functionCall.GetProperty("name").GetString()!;
            var args = functionCall.GetProperty("args");

            // Convert args to Dictionary<string, object>
            var argsDict = new Dictionary<string, object>();
            foreach (var prop in args.EnumerateObject())
            {
                argsDict[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString()!,
                    JsonValueKind.Number => prop.Value.GetInt32(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => prop.Value.Clone()
                };
            }

            return new RagLLMResult
            {
                ToolCall = new RagToolCall
                {
                    Name = name,
                    Arguments = argsDict
                }
            };
        }

        // Regular text response
        if (firstPart.TryGetProperty("text", out var text))
        {
            return new RagLLMResult
            {
                FinalAnswer = text.GetString()
            };
        }

        throw new InvalidOperationException("Unexpected response format from Gemini API");
    }
}
