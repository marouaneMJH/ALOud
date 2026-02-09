using ALOud.Services.Rag.Models;

public interface ILlmGenerationService
{
    Task<string> GenerateAsync(
        RagContextPayload context,
        string userQuery,
        CancellationToken cancellationToken = default
    );
}
