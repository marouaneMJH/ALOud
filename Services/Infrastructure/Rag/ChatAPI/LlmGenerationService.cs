//  Take a prepared RAG context and a user query, call the LLM, return a final answer.
using System.Text;
using ALOud.Services.Rag.Clients;
using ALOud.Services.Rag.Models;

public sealed class LlmGenerationService : ILlmGenerationService
{
    private readonly IRagLLMClient _llm;

    public LlmGenerationService(IRagLLMClient llm)
    {
        _llm = llm;
    }

    public async Task<string> GenerateAsync(
        RagContextPayload context,
        string userQuery,
        CancellationToken cancellationToken = default)
    {
        var result = await _llm.ExecuteAsync(new RagLLMRequest
        {
            SystemPrompt = context.SystemContext,
            UserMessage = userQuery,
            Context = new
            {
                knowledge = context.KnowledgeContext,
                live = context.LiveContext
            }
        });

        return result.FinalAnswer
            ?? "I couldn’t generate an answer based on the provided context.";
    }
}
