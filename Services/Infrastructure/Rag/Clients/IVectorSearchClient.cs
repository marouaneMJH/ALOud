using ALOud.Models;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.Clients;

public interface IVectorSearchClient
{
    Task<IReadOnlyList<RagRetrievedChunk>> SearchAsync(
        float[] vector,
        int topK,
        QdrantFilter? filter,
        CancellationToken cancellationToken = default
    );
}
