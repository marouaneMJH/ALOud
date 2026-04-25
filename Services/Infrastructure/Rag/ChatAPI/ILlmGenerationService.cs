using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.ChatAPI;


public interface ILlmGenerationService
{
    Task<string> GenerateAsync(
        RagContextPayload context,
        string userQuery,
        CancellationToken cancellationToken = default
    );
}
