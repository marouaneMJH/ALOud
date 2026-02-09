using ALOud.Services.Rag.Models;

public interface IEmbeddingIndexService
{
    Task<IReadOnlyList<VectorRecord>> EmbedAsync(
        IReadOnlyList<RagDocumentChunk> chunks,
        CancellationToken cancellationToken = default
    );
}
