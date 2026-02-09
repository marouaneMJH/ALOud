using ALOud.Services.Rag.Models;

public interface IChunkingService
{
    IReadOnlyList<RagDocumentChunk> Chunk(
        PerfumeRagSource source,
        string document
    );
}
