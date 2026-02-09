using ALOud.Services.Rag.Models;

public interface ILiveContextProvider
{
    Task<LiveContextPayload?> BuildAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

