namespace ALOud.Services.Infrastructure.Rag.Models;

public class RagDocumentChunk
{
    public Guid SourceId { get; init; }      // PerfumeId
    public int ChunkIndex { get; init; }
    public string Content { get; init; } = string.Empty;

    // Metadata for filtering later
    public string? Brand { get; init; }
    public string? GenderProfile { get; init; }
    public string? PriceRange { get; init; }
    public string? Sillage { get; init; }
    public string? Longevity { get; init; }
    public string? ImageUrl { get; init; }
}
