namespace ALOud.Services.Rag.Models;



public sealed class RagLLMResult
{
    public string? FinalAnswer { get; init; }
    public RagToolCall? ToolCall { get; init; }
    public bool IsToolCall => ToolCall != null;


}