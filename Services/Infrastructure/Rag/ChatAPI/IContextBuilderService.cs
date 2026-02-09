using ALOud.Services.Rag.Models;

public interface IContextBuilderService
{
    RagContextPayload Build(
        string userQuery,
        IReadOnlyList<RagRetrievedChunk> retrievedChunks,
        string? liveContext
    );
}
