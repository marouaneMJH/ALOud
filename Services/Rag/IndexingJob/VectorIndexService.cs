// Persist vector records into a vector database and manage their lifecycle.


using ALOud.Services.Rag.Clients;
using ALOud.Services.Rag.Models;

public class VectorIndexService : IVectorIndexService
{
    private readonly IVectorDbClient _vectorDbClient;

    public VectorIndexService(IVectorDbClient vectorDbClient)
    {
        _vectorDbClient = vectorDbClient;
    }

    public async Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default)
    {
        if (records.Count == 0)
            return;

        await _vectorDbClient.UpsertAsync(records, cancellationToken);
    }

    public async Task DeleteBySourceIdAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        await _vectorDbClient.DeleteByFilterAsync(
            field: "sourceId",
            value: sourceId.ToString(),
            cancellationToken
        );
    }
}
