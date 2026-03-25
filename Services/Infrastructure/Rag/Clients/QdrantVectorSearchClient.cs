using ALOud.Models;
using System.Text.Json;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Rag.Clients;

public sealed class QdrantVectorSearchClient : IVectorSearchClient
{
    private readonly HttpClient _http;
    private readonly QdrantSettings _settings;
    private readonly ILogger<QdrantVectorSearchClient> _logger;

    public QdrantVectorSearchClient(
        HttpClient http,
        QdrantSettings settings,
        ILogger<QdrantVectorSearchClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
        _http.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<IReadOnlyList<RagRetrievedChunk>> SearchAsync(
        float[] vector,
        int topK,
        Dictionary<string, object>? filter,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            vector,
            limit = topK,
            with_payload = true,
            filter = filter == null ? null : new
            {
                must = filter.Select(f => new
                {
                    key = f.Key,
                    match = new { value = f.Value }
                })
            }
        };

        var response = await _http.PostAsJsonAsync(
            $"collections/{_settings.Collection}/points/search",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var results = json.RootElement
            .GetProperty("result")
            .EnumerateArray()
            .Select(p => new RagRetrievedChunk
            {
                Id = p.GetProperty("id").ToString(),
                Score = p.GetProperty("score").GetSingle(),
                Content = p.GetProperty("payload").GetProperty("content").GetString()!,
                Metadata = p.GetProperty("payload")
                    .EnumerateObject()
                    .Where(x => x.Name != "content")
                    .ToDictionary(x => x.Name, x => (object)x.Value.ToString()!)
            })
            .ToList();

        _logger.LogInformation(
            "Qdrant search returned {Count} results",
            results.Count);

        return results;
    }
}
