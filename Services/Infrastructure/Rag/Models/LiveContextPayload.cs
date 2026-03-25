namespace ALOud.Services.Infrastructure.Rag.Models;


/// <summary>
/// Represents dynamic, runtime context injected into RAG
/// during the augmentation step (not indexed, not embedded).
/// </summary>
public sealed class LiveContextPayload
{
    /// <summary>
    /// Logical name of the live context.
    /// Example: "cart", "user_profile", "inventory"
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Structured data object.
    /// Must be serializable and deterministic.
    /// </summary>
    public object Data { get; init; } = default!;

    /// <summary>
    /// Optional instructions for the LLM on how to use this data.
    /// Example:
    /// "This data is authoritative. Do not hallucinate values."
    /// </summary>
    public string? UsageHint { get; init; }

    /// <summary>
    /// Indicates whether this context is authoritative.
    /// Authoritative data must override retrieved knowledge.
    /// </summary>
    public bool IsAuthoritative { get; init; } = true;
}
