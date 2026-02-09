using ALOud.Services.Rag.Models;

public interface IVectorDbClient
{
    Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default
    );

    Task DeleteByFilterAsync(
        string field,
        string value,
        CancellationToken cancellationToken = default
    );
}
