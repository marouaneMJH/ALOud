namespace ALOud.Services.Rag.Models;

public sealed class QdrantFilter
{
    public List<FilterCondition> Must { get; init; } = new();
    public List<FilterCondition> MustNot { get; init; } = new();
}

public sealed class FilterCondition
{
    public required string Key { get; init; }
    public required object Value { get; init; }
}
