using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Rag.Clients;

/// <summary>
/// Factory for creating LLM clients based on environment configuration
/// </summary>
public class LLMClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly LLMProviderConfig _config;

    public LLMClientFactory(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        _config = new LLMProviderConfig();
    }

    public IRagLLMClient CreateClient()
    {
        var httpClient = _httpClientFactory.CreateClient("LLMClient");

        return _config.Provider.ToLower() switch
        {
            "gemini" => new GeminiLLMClient(
                httpClient,
                _loggerFactory.CreateLogger<GeminiLLMClient>(),
                _config.Gemini),

            "groq" => new GroqLLMClient(
                httpClient,
                _loggerFactory.CreateLogger<GroqLLMClient>(),
                _config.Groq),

            "grok" => new GrokLLMClient(
                httpClient,
                _loggerFactory.CreateLogger<GrokLLMClient>(),
                _config.Grok),

            _ => throw new InvalidOperationException(
                $"Unknown LLM provider: {_config.Provider}. Valid options: gemini, groq, grok")
        };
    }

    public string GetCurrentProvider() => _config.Provider;
}
