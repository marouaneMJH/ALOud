namespace ALOud.Services.Infrastructure.Rag.Clients.LLMClient;

public interface ILlmClient
{
    Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default
    );
}
