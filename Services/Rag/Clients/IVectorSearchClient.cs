using ALOud.Services.Rag.Models;

public interface IVectorSearchClient
{
    Task<IReadOnlyList<RagRetrievedChunk>> SearchAsync(
        float[] vector,
        int topK,
        Dictionary<string, object>? filter,
        CancellationToken cancellationToken = default
    );
}
