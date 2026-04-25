using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;

public interface IProductDataExtractor
{
    Task<IReadOnlyList<PerfumeRagSource>> ExtractAllAsync(
        CancellationToken cancellationToken = default
    );
}
