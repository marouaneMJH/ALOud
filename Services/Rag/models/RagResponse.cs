namespace ALOud.Services.Rag.Models;

public sealed class RagResponse
{
    public required string Answer { get; init; }
    public object? CartSnapshot { get; init; }
}