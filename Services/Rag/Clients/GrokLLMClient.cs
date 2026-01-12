using System.Text.Json;
using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class GrokLLMClient : BaseLLMClient
{
    private readonly GrokSettings _settings;

    public GrokLLMClient(HttpClient http, ILogger<GrokLLMClient> logger, GrokSettings settings)
        : base(http, logger, settings.ApiKeyEnvVar)
    {
        _settings = settings;
    }

    protected override string GetProviderName() => "Grok";

    protected override string BuildRequestUrl()
    {
        return "https://api.x.ai/v1/chat/completions";
    }

    protected override void ConfigureRequest(HttpRequestMessage request, string payloadJson)
    {
        base.ConfigureRequest(request, payloadJson);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
    }

    protected override object BuildPayload(RagLLMRequest request)
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

        // Add conversation history if present
        if (request.ConversationHistory != null)
        {
            foreach (var turn in request.ConversationHistory)
            {
                if (turn.Role == "assistant" && turn.FunctionCallName != null)
                {
                    // Assistant called a function
                    messages.Add(new
                    {
                        role = "assistant",
                        content = (string?)null,
                        tool_calls = new[]
                        {
                            new
                            {
                                id = $"call_{Guid.NewGuid():N}",
                                type = "function",
                                function = new
                                {
                                    name = turn.FunctionCallName,
                                    arguments = JsonSerializer.Serialize(turn.FunctionCallArgs)
                                }
                            }
                        }
                    });
                }
                else if (turn.Role == "tool")
                {
                    // Function response
                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = $"call_{Guid.NewGuid():N}",
                        content = JsonSerializer.Serialize(turn.FunctionResponse)
                    });
                }
            }
        }

        var payload = new
        {
            model = _settings.Model,
            messages = messages.ToArray(),
            temperature = 0.1,
            max_tokens = 2048
        };

        // Add tools if provided
        if (request.Tools != null && request.Tools.Any())
        {
            var tools = request.Tools.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.ParametersSchema
                }
            }).ToArray();

            return new
            {
                model = _settings.Model,
                messages = messages.ToArray(),
                tools,
                tool_choice = "auto",
                temperature = 0.1,
                max_tokens = 2048
            };
        }

        return payload;
    }

    protected override RagLLMResult ParseResponse(JsonDocument doc)
    {
        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("No choices returned from Grok API");
        }

        var choice = choices[0];
        var message = choice.GetProperty("message");

        // Check if it's a tool call
        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
        {
            var toolCall = toolCalls[0];
            var function = toolCall.GetProperty("function");
            var name = function.GetProperty("name").GetString()!;
            var argsJson = function.GetProperty("arguments").GetString()!;

            // Parse arguments JSON
            var argsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(argsJson)
                ?? new Dictionary<string, object>();

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
        if (message.TryGetProperty("content", out var content) && content.ValueKind != JsonValueKind.Null)
        {
            return new RagLLMResult
            {
                FinalAnswer = content.GetString()
            };
        }

        throw new InvalidOperationException("Unexpected response format from Grok API");
    }
}
