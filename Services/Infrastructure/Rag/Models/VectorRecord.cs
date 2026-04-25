namespace ALOud.Services.Infrastructure.Rag.Models;

public class VectorRecord
{
    public string Id { get; init; } = string.Empty;
    public float[] Vector { get; init; } = Array.Empty<float>();
    public string Content { get; init; } = string.Empty;

    public Dictionary<string, object> Metadata { get; init; } = new();
}
