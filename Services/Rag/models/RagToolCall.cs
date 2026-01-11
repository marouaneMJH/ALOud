namespace ALOud.Services.Rag.Models;


public sealed class RagToolCall
{
    public required string Name { get; init; }
    public required Dictionary<string, object> Argument { get; init; }
}