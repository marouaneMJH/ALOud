using System.Text.Json;

namespace ALOud.Services.Rag.Clients;

public sealed class OllamaEmbeddingClient : IEmbeddingClient
{
    private readonly HttpClient _http;
    private readonly ILogger<OllamaEmbeddingClient> _logger;

    public OllamaEmbeddingClient(
        HttpClient http,
        ILogger<OllamaEmbeddingClient> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri("http://localhost:11434/");
    }

    public async Task<float[]> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/embeddings",
            new
            {
                model = "nomic-embed-text",
                prompt = input
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var embedding = json
            .RootElement
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        _logger.LogDebug("Generated FREE embedding with {Dim} dimensions", embedding.Length);

        return embedding;
    }
}
