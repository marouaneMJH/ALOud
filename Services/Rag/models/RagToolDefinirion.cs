


namespace ALOud.Services.Rag.Models;


public sealed class RagToolDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required object ParametersSchema { get; init; }

}