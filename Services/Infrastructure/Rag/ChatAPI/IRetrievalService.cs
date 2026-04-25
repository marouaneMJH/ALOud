using ALOud.Models;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.ChatAPI;


public interface IRetrievalService
{
    Task<IReadOnlyList<RagRetrievedChunk>> RetrieveAsync(
        float[] queryVector,
        int topK = 5,
        RetrievalFilter? filter = null,
        CancellationToken cancellationToken = default
    );
}
