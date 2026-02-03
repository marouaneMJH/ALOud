using System.Text.Json;
using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag.Clients;

public sealed class QdrantVectorDbClient : IVectorDbClient
{
    private readonly HttpClient _http;
    private readonly QdrantSettings _settings;
    private readonly ILogger<QdrantVectorDbClient> _logger;

    public QdrantVectorDbClient(
        HttpClient http,
        QdrantSettings settings,
        ILogger<QdrantVectorDbClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
        _http.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default)
    {
        if (records.Count == 0)
            return;

        var payload = new
        {
            points = records.Select(r => new
            {
                id = r.Id,
                vector = r.Vector,
                payload = r.Metadata.Append(
                    new KeyValuePair<string, object>("content", r.Content)
                ).ToDictionary(k => k.Key, v => v.Value)
            })
        };
        // fix: Bas request ?
        var response = await _http.PutAsJsonAsync(
            $"collections/{_settings.Collection}/points",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "Upserted {Count} vectors into Qdrant collection '{Collection}'",
            records.Count,
            _settings.Collection);
    }

    public async Task DeleteByFilterAsync(
        string field,
        string value,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            filter = new
            {
                must = new[]
                {
                    new
                    {
                        key = field,
                        match = new { value }
                    }
                }
            }
        };

        var response = await _http.PostAsJsonAsync(
            $"collections/{_settings.Collection}/points/delete",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "Deleted vectors from Qdrant where {Field} = {Value}",
            field,
            value);
    }
}
