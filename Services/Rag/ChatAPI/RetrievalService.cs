// Given a query embedding, fetch the Top-K most relevant document chunks
// from the vector database, optionally filtered by metadata.

using ALOud.Services.Rag.Clients;
using ALOud.Services.Rag.Models;

public class RetrievalService : IRetrievalService
{
    private readonly IVectorSearchClient _vectorSearchClient;

    public RetrievalService(IVectorSearchClient vectorSearchClient)
    {
        _vectorSearchClient = vectorSearchClient;
    }

    public async Task<IReadOnlyList<RagRetrievedChunk>> RetrieveAsync(
        float[] queryVector,
        int topK = 5,
        RetrievalFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (queryVector == null || queryVector.Length == 0)
            throw new ArgumentException("Query vector is empty", nameof(queryVector));

        var dbFilter = BuildFilter(filter);

        return await _vectorSearchClient.SearchAsync(
            vector: queryVector,
            topK: topK,
            filter: dbFilter,
            cancellationToken: cancellationToken
        );
    }

    private static Dictionary<string, object>? BuildFilter(RetrievalFilter? filter)
    {
        if (filter == null)
            return null;

        var dict = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            dict["brand"] = filter.Brand;

        if (!string.IsNullOrWhiteSpace(filter.GenderProfile))
            dict["genderProfile"] = filter.GenderProfile;

        if (!string.IsNullOrWhiteSpace(filter.PriceRange))
            dict["priceRange"] = filter.PriceRange;

        return dict.Count > 0 ? dict : null;
    }
}
