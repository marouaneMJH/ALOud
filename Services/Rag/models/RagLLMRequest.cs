
namespace ALOud.Services.Rag.Models;

public sealed class RagLLMRequest
{
    public required string SystemPrompt { get; init; }
    public required string UserMessage { get; init; }
    public object? Context { get; init; }

    public IReadOnlyList<RagToolDefinition>? Tools { get; init; }


}