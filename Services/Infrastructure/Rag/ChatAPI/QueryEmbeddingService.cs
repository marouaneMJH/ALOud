// Convert a user query string into a vector embedding usable by the retrieval layer.

using ALOud.Services.Rag.Clients;

public class QueryEmbeddingService : IQueryEmbeddingService
{
    private readonly IEmbeddingClient _embeddingClient;

    public QueryEmbeddingService(IEmbeddingClient embeddingClient)
    {
        _embeddingClient = embeddingClient;
    }

    public async Task<float[]> EmbedAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));

        var normalizedQuery = Normalize(query);

        return await _embeddingClient.CreateEmbeddingAsync(
            normalizedQuery,
            cancellationToken
        );
    }

    // TODO: Lowercasing, Stopword hints, Query expansion
    private static string Normalize(string query)
    {
        return query.Trim();
    }
}
