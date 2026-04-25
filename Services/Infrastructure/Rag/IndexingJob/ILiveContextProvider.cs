using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public interface ILiveContextProvider
{
    Task<LiveContextPayload?> BuildAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

