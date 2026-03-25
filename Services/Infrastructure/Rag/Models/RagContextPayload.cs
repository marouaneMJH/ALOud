namespace ALOud.Services.Infrastructure.Rag.Models;

public class RagContextPayload
{
    public string SystemContext { get; init; } = string.Empty;
    public string KnowledgeContext { get; init; } = string.Empty;
    public string LiveContext { get; init; } = string.Empty;
}
