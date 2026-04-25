using ALOud.Models;
using System.Text;
using ALOud.Services.Rag;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.ChatAPI;


public class ContextBuilderService : IContextBuilderService
{
    public RagContextPayload Build(
        string userQuery,
        IReadOnlyList<RagRetrievedChunk> retrievedChunks,
        string? liveContext)
    {
        return new RagContextPayload
        {
            SystemContext = SystemPrompts.ShoppingAssistant,
            KnowledgeContext = BuildKnowledgeContext(retrievedChunks),
            LiveContext = BuildLiveContext(liveContext)
        };
    }

    private static string BuildKnowledgeContext(
        IReadOnlyList<RagRetrievedChunk> chunks)
    {
        if (chunks.Count == 0)
            return "No relevant product knowledge found.";

        var sb = new StringBuilder();
        sb.AppendLine("PRODUCT KNOWLEDGE:");

        foreach (var chunk in chunks)
        {
            sb.AppendLine("---");
            sb.AppendLine(chunk.Content.Trim());
        }

        return sb.ToString();
    }

    private static string BuildLiveContext(string? liveContext)
    {
        if (string.IsNullOrWhiteSpace(liveContext))
            return "No live cart data available.";

        return $"LIVE CART DATA:\n{liveContext.Trim()}";
    }
}
