using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public interface IEmbeddingIndexService
{
    Task<IReadOnlyList<VectorRecord>> EmbedAsync(
        IReadOnlyList<RagDocumentChunk> chunks,
        CancellationToken cancellationToken = default
    );
}
