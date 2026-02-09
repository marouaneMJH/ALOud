using ALOud.Services.Rag.Models;

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
