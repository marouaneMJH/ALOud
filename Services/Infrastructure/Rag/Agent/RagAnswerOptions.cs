namespace ALOud.Services.Infrastructure.Rag.Agent;

public sealed class RagAnswerOptions
{
    public bool Debug { get; set; } = false;
    public int TopK { get; set; } = 5;
}
