using ALOud.Models;
namespace ALOud.Services.Infrastructure.Rag.Models;

public sealed class RagDebugResult
{
    public string Answer { get; set; } = string.Empty;
    public IReadOnlyList<RagRetrievedChunk> RetrievedChunks { get; set; }
        = Array.Empty<RagRetrievedChunk>();
}