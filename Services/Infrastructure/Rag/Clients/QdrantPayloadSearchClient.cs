using ALOud.Models;
using System.Text.Json;
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.Services.Infrastructure.Rag.Clients;

namespace ALOud.Services.Rag.Clients;

public sealed class QdrantPayloadSearchClient : IPayloadSearchClient
{
    private readonly HttpClient _http;
    private readonly QdrantSettings _settings;
    private readonly ILogger<QdrantPayloadSearchClient> _logger;

    public QdrantPayloadSearchClient(
        HttpClient http,
        QdrantSettings settings,
        ILogger<QdrantPayloadSearchClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
        _http.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<IReadOnlyList<RagRetrievedChunk>> SearchByFilterAsync(
        int topK,
        QdrantFilter? filter,
        CancellationToken cancellationToken = default)
    {
        object? qdrantFilter = null;
        
        if (filter != null && (filter.Must.Count > 0 || filter.MustNot.Count > 0))
        {
            var filterObj = new Dictionary<string, object>();
            
            if (filter.Must.Count > 0)
            {
                filterObj["must"] = filter.Must.Select(c => new
                {
                    key = c.Key,
                    match = new { value = c.Value }
                }).ToArray();
            }
            
            if (filter.MustNot.Count > 0)
            {
                filterObj["must_not"] = filter.MustNot.Select(c => new
                {
                    key = c.Key,
                    match = new { value = c.Value }
                }).ToArray();
            }
            
            qdrantFilter = filterObj;
        }

        var payload = new
        {
            limit = topK,
            with_payload = true,
            with_vector = false,
            filter = qdrantFilter
        };

        var response = await _http.PostAsJsonAsync(
            $"collections/{_settings.Collection}/points/scroll",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var points = json.RootElement
            .GetProperty("result")
            .GetProperty("points")
            .EnumerateArray();

        var results = points
            .Select(point =>
            {
                var pointPayload = point.GetProperty("payload");
                var content = pointPayload.TryGetProperty("content", out var contentElement)
                    ? contentElement.GetString() ?? string.Empty
                    : string.Empty;

                var metadata = pointPayload
                    .EnumerateObject()
                    .Where(x => x.Name != "content")
                    .ToDictionary(x => x.Name, x => (object)x.Value.ToString()!);

                return new RagRetrievedChunk
                {
                    Id = point.GetProperty("id").ToString(),
                    Score = 0f,
                    Content = content,
                    Metadata = metadata
                };
            })
            .ToList();

        _logger.LogInformation(
            "Qdrant payload search returned {Count} results",
            results.Count);

        return results;
    }
}
