using ALOud.Services.Rag.Models;

public interface IPayloadSearchClient
{
    Task<IReadOnlyList<RagRetrievedChunk>> SearchByFilterAsync(
        int topK,
        QdrantFilter? filter,
        CancellationToken cancellationToken = default
    );
}
