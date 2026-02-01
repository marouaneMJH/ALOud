//  Take a prepared RAG context and a user query, call the LLM, return a final answer.
using System.Text;
using ALOud.Services.Rag.Clients;
using ALOud.Services.Rag.Models;

public class LlmGenerationService : ILlmGenerationService
{
    private readonly ILlmClient _llmClient;

    public LlmGenerationService(ILlmClient llmClient)
    {
        _llmClient = llmClient;
    }

    public async Task<string> GenerateAsync(
        RagContextPayload context,
        string userQuery,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
            throw new ArgumentException("User query cannot be empty", nameof(userQuery));

        var userPrompt = BuildUserPrompt(userQuery, context);

        return await _llmClient.GenerateAsync(
            systemPrompt: context.SystemContext,
            userPrompt: userPrompt,
            cancellationToken: cancellationToken
        );
    }

    private static string BuildUserPrompt(
        string userQuery,
        RagContextPayload context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("USER QUESTION:");
        sb.AppendLine(userQuery.Trim());
        sb.AppendLine();

        sb.AppendLine("CONTEXT:");
        sb.AppendLine(context.KnowledgeContext);
        sb.AppendLine();
        sb.AppendLine(context.LiveContext);

        return sb.ToString().Trim();
    }
}
