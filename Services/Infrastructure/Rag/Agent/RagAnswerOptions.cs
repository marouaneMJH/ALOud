namespace ALOud.Services.Rag.Models;

public sealed class RagAnswerOptions
{
    public bool Debug { get; set; } = false;
    public int TopK { get; set; } = 5;
}
