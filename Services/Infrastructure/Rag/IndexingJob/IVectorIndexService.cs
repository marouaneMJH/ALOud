using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public interface IVectorIndexService
{
    Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default
    );

    Task DeleteBySourceIdAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default
    );
}
