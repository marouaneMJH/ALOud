

using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.ChatAPI;


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
