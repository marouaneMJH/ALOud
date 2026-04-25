using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public interface IChunkingService
{
    IReadOnlyList<RagDocumentChunk> Chunk(
        PerfumeRagSource source,
        string document
    );
}
