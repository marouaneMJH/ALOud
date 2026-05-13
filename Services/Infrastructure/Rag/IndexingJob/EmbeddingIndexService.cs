// Convert document chunks into embeddings, ready to be stored in a vector database.
using System.Security.Cryptography;
using System.Text;
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
                Id = DeriveChunkId(chunk.SourceId, chunk.ChunkIndex).ToString(),
                Vector = vector,
                Content = chunk.Content,
                Metadata = BuildMetadata(chunk)
            });
        }

        return results;
    }

    // Produces a stable UUID for each (perfumeId, chunkIndex) pair.
    // Qdrant requires point IDs to be either a UUID or an unsigned integer.
    private static Guid DeriveChunkId(Guid sourceId, int chunkIndex)
    {
        var input = Encoding.UTF8.GetBytes($"{sourceId}:{chunkIndex}");
        var hash = MD5.HashData(input);
        return new Guid(hash);
    }

    private static Dictionary<string, object> BuildMetadata(
        RagDocumentChunk chunk)
    {
        var meta = new Dictionary<string, object>
        {
            ["sourceId"]      = chunk.SourceId.ToString(),
            ["chunkIndex"]    = chunk.ChunkIndex,
            ["brand"]         = chunk.Brand         ?? string.Empty,
            ["genderProfile"] = chunk.GenderProfile ?? string.Empty,
            ["priceRange"]    = chunk.PriceRange    ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(chunk.Sillage))
            meta["sillage"] = chunk.Sillage;

        if (!string.IsNullOrWhiteSpace(chunk.Longevity))
            meta["longevity"] = chunk.Longevity;

        if (!string.IsNullOrWhiteSpace(chunk.ImageUrl))
            meta["imageUrl"] = chunk.ImageUrl;

        return meta;
    }
}
