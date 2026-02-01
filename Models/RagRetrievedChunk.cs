public class RagRetrievedChunk
{
    public string Id { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public float Score { get; init; }

    public Dictionary<string, object> Metadata { get; init; } = new();
}
