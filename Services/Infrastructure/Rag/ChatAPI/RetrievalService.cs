using ALOud.Models;
// Given a query embedding, fetch the Top-K most relevant document chunks
// from the vector database, optionally filtered by metadata.

using ALOud.Services.Rag.Clients;
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.Services.Infrastructure.Rag.Clients;

namespace ALOud.Services.Infrastructure.Rag.ChatAPI;


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

    private static QdrantFilter? BuildFilter(RetrievalFilter? filter)
    {
        if (filter == null)
            return null;

        var qdrantFilter = new QdrantFilter();

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            qdrantFilter.Must.Add(new FilterCondition { Key = "brand", Value = filter.Brand });

        if (!string.IsNullOrWhiteSpace(filter.GenderProfile))
            qdrantFilter.Must.Add(new FilterCondition { Key = "genderProfile", Value = filter.GenderProfile });

        if (!string.IsNullOrWhiteSpace(filter.PriceRange))
            qdrantFilter.Must.Add(new FilterCondition { Key = "priceRange", Value = filter.PriceRange });

        return qdrantFilter.Must.Count > 0 ? qdrantFilter : null;
    }
}
