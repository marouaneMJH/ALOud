using ALOud.Models;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.ChatAPI;


public interface IContextBuilderService
{
    RagContextPayload Build(
        string userQuery,
        IReadOnlyList<RagRetrievedChunk> retrievedChunks,
        string? liveContext
    );
}
