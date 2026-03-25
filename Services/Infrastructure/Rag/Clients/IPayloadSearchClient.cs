using ALOud.Models;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.Clients
{
    public interface IPayloadSearchClient
    {
        Task<IReadOnlyList<RagRetrievedChunk>> SearchByFilterAsync(
            int topK,
            QdrantFilter? filter,
            CancellationToken cancellationToken = default
        );
    }
}
