
namespace ALOud.Services.Infrastructure.Rag.Models;

public sealed class RagLLMRequest
{
    public required string SystemPrompt { get; init; }
    public required string UserMessage { get; init; }
    public object? Context { get; init; }
    public List<ConversationTurn>? ConversationHistory { get; init; }

    public IReadOnlyList<RagToolDefinition>? Tools { get; init; }
}

public sealed class ConversationTurn
{
    public required string Role { get; init; } // "user", "model", "function"
    public string? Text { get; init; }
    public string? FunctionName { get; init; }
    public object? FunctionResponse { get; init; }
    public string? FunctionCallName { get; init; }
    public Dictionary<string, object>? FunctionCallArgs { get; init; }
}