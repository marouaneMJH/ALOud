

using ALOud.Services.Rag.Models;

public interface IChatOrchestratorService
{
    Task<string> HandleAsync(
        Guid userId,
        string message,
        CancellationToken cancellationToken = default
    );

    Task<RagDebugResult> HandleWithDebugAsync(
         Guid userId,
         string message,
         CancellationToken cancellationToken = default
     );
}
