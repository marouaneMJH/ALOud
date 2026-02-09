using System.Net;
using System.Text.Json;

using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag.IndexingJob;

public sealed class QdrantBootstrapService
{
    private readonly HttpClient _http;
    private readonly ILogger<QdrantBootstrapService> _logger;
    // TODO connect other attribues to setttings
    private const string CollectionName = "perfumes";
    private const int VectorSize = 768; // nomic-embed-text

    public QdrantBootstrapService(
        HttpClient http,
    QdrantSettings settings,
        ILogger<QdrantBootstrapService> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri(settings.BaseUrl);
    }

    public async Task EnsureCollectionExistsAsync(
        CancellationToken cancellationToken = default)
    {
        if (await CollectionExistsAsync(cancellationToken))
        {
            _logger.LogInformation("Qdrant collection '{Collection}' already exists", CollectionName);
            return;
        }

        _logger.LogInformation("Creating Qdrant collection '{Collection}'", CollectionName);

        var payload = new
        {
            vectors = new
            {
                size = VectorSize,
                distance = "Cosine"
            }
        };

        var response = await _http.PutAsJsonAsync(
            $"collections/{CollectionName}",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Qdrant collection '{Collection}' created successfully", CollectionName);
    }

    private async Task<bool> CollectionExistsAsync(
        CancellationToken cancellationToken)
    {
        var response = await _http.GetAsync(
            $"collections/{CollectionName}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}
