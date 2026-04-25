namespace ALOud.Services.Infrastructure.Rag.Models;


public sealed class RagToolCall
{
    public required string Name { get; init; }
    public required Dictionary<string, object> Arguments { get; init; }
}