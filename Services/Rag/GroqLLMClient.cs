using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ALOud.Services.Rag;

public sealed class GroqLLMClient : IRagLLMClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    private readonly ILogger<GroqLLMClient> _logger;
    public GroqLLMClient(HttpClient http, ILogger<GroqLLMClient> logger)
    {
        _http = http;
        _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? throw new InvalidCastException("GROQ_API_KEY missing");
        _logger = logger;
    }
    public async Task<RagLLMResult> ExecuteAsync(RagLLMRequest request)
    {
        var payload = BuildPayload(request);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.groq.com/openai/v1/chat/completions"
        );


        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
        Encoding.UTF8,
        "application/json"
        );

        var response = await _http.SendAsync(httpRequest);
        _logger.LogInformation(JsonSerializer.Serialize(response));
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        return ParseResponse(doc);

    }


    private static object BuildPayload(RagLLMRequest request)
    {
        var messages = new List<object>
        {
            new {role =  "system", content = request.SystemPrompt},
            new {role =  "user", content = request.UserMessage}
        };

        if (request.Context != null)
        {
            messages.Insert(1, new
            {
                role = "system",
                content = JsonSerializer.Serialize(request.Context)
            });
        }

        return new
        {
            model = "llama-3.1-8b-instant",
            temperature = 0.1,
            messages,
            tools = request.Tools,
            tool_choice = "auto"
        };
    }


    private static RagLLMResult ParseResponse(JsonDocument doc)
    {
        var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

        if (message.TryGetProperty("tool_calls", out var toolCalls))
        {
            var call = toolCalls[0];
            return new RagLLMResult
            {
                ToolCall = new RagToolCall
                {
                    Name = call.GetProperty("function").GetProperty("name").GetString()!,
                    Arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        call.GetProperty("function").GetProperty("arguments")
                    ) ?? new()
                }
            };
        }

        return new RagLLMResult
        {
            FinalAnswer = message.GetProperty("content").GetString()
        };
    }
}