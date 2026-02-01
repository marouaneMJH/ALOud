using ALOud.Services.Rag.Models;

public interface IRetrievalService
{
    Task<IReadOnlyList<RagRetrievedChunk>> RetrieveAsync(
        float[] queryVector,
        int topK = 5,
        RetrievalFilter? filter = null,
        CancellationToken cancellationToken = default
    );
}
