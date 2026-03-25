// Convert document chunks into embeddings, ready to be stored in a vector database.
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.Services.Rag.Clients;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public class EmbeddingIndexService : IEmbeddingIndexService
{
    private readonly IEmbeddingClient _embeddingClient;

    public EmbeddingIndexService(IEmbeddingClient embeddingClient)
    {
        _embeddingClient = embeddingClient;
    }

    public async Task<IReadOnlyList<VectorRecord>> EmbedAsync(
        IReadOnlyList<RagDocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        var results = new List<VectorRecord>();

        foreach (var chunk in chunks)
        {
            var vector = await _embeddingClient.CreateEmbeddingAsync(
                chunk.Content,
                cancellationToken
            );

            results.Add(new VectorRecord
            {
                Id = chunk.SourceId.ToString(),
                Vector = vector,
                Content = chunk.Content,
                Metadata = BuildMetadata(chunk)
            });
        }

        return results;
    }

    private static Dictionary<string, object> BuildMetadata(
        RagDocumentChunk chunk)
    {
        return new Dictionary<string, object>
        {
            ["sourceId"] = chunk.SourceId.ToString(),
            ["chunkIndex"] = chunk.ChunkIndex,
            ["brand"] = chunk.Brand ?? string.Empty,
            ["genderProfile"] = chunk.GenderProfile ?? string.Empty,
            ["priceRange"] = chunk.PriceRange ?? string.Empty
        };
    }
}
