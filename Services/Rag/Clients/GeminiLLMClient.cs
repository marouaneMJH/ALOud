using System.Text;
using System.Text.Json;
using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class GeminiLLMClient : BaseLLMClient
{
    private readonly GeminiSettings _settings;

    public GeminiLLMClient(HttpClient http, ILogger<GeminiLLMClient> logger, GeminiSettings settings)
        : base(http, logger, settings.ApiKeyEnvVar)
    {
        _settings = settings;
    }

    protected override string GetProviderName() => "Gemini";

    protected override string BuildRequestUrl()
    {
        return $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_apiKey}";
    }

    protected override object BuildPayload(RagLLMRequest request)
    {
        var systemInstruction = new
        {
            parts = new[] { new { text = request.SystemPrompt } }
        };

        var contentsList = new List<object>();

        // Add initial user message with cart context
        var userContent = BuildUserContent(request);
        contentsList.Add(new
        {
            role = "user",
            parts = new[] { new { text = userContent } }
        });

        // Add conversation history if present
        if (request.ConversationHistory != null)
        {
            foreach (var turn in request.ConversationHistory)
            {
                if (turn.Role == "model" && turn.FunctionCallName != null)
                {
                    // Model called a function
                    contentsList.Add(new
                    {
                        role = "model",
                        parts = new[] { new { functionCall = new { name = turn.FunctionCallName, args = turn.FunctionCallArgs } } }
                    });
                }
                else if (turn.Role == "function")
                {
                    // Function response
                    contentsList.Add(new
                    {
                        role = "user",
                        parts = new[] { new { functionResponse = new { name = turn.FunctionName, response = turn.FunctionResponse } } }
                    });
                }
            }
        }

        var payload = new
        {
            systemInstruction,
            contents = contentsList.ToArray(),
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
                contents = contentsList.ToArray(),
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

    protected override RagLLMResult ParseResponse(JsonDocument doc)
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
