using ALOud.Services.Infrastructure.Rag.Models;
// Split a semantic document string into embedding-ready chunks
// while preserving meaning and metadata.



namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public class ChunkingService : IChunkingService
{
    private const int MaxChunkSize = 900;
    private const int OverlapSize = 120;

    public IReadOnlyList<RagDocumentChunk> Chunk(
        PerfumeRagSource source,
        string document)
    {
        var chunks = new List<RagDocumentChunk>();

        if (string.IsNullOrWhiteSpace(document))
            return chunks;

        var paragraphs = document
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

        var buffer = string.Empty;
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            if ((buffer.Length + paragraph.Length) <= MaxChunkSize)
            {
                buffer += paragraph + "\n\n";
                continue;
            }

            // Flush current buffer
            chunks.Add(CreateChunk(
                source,
                chunkIndex++,
                buffer.Trim()
            ));

            // Apply overlap
            buffer = buffer.Length > OverlapSize
                ? buffer[^OverlapSize..] + paragraph + "\n\n"
                : paragraph + "\n\n";
        }

        // Final flush
        if (!string.IsNullOrWhiteSpace(buffer))
        {
            chunks.Add(CreateChunk(
                source,
                chunkIndex,
                buffer.Trim()
            ));
        }

        return chunks;
    }

    private static RagDocumentChunk CreateChunk(
        PerfumeRagSource source,
        int index,
        string content)
    {
        return new RagDocumentChunk
        {
            SourceId = source.Id,
            ChunkIndex = index,
            Content = content,

            Brand = source.Brand,
            GenderProfile = source.GenderProfile,
            PriceRange = source.PriceRange
        };
    }
}
