using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class RagCartService
{
    private const int MaxToolCalls = 3;

    private readonly RagContextBuilder _contextBuilder;
    private readonly RagToolDispatcher _dispatcher;
    private readonly IRagLLMClient _llm;

    public RagCartService(
        RagContextBuilder contextBuilder,
        RagToolDispatcher dispatcher,
        IRagLLMClient llm)
    {
        _contextBuilder = contextBuilder;
        _dispatcher = dispatcher;
        _llm = llm;
    }

    public async Task<RagResponse> HandleAsync(string userMessage)
    {
        var baseContext = await _contextBuilder.BuildAsync();
        var conversationContext = new List<object>
        {
            new { role = "system", content = baseContext }
        };

        for (var step = 0; step < MaxToolCalls; step++)
        {
            var llmResult = await _llm.ExecuteAsync(new RagLLMRequest
            {
                SystemPrompt = SystemPrompts.CartAssistant,
                UserMessage = userMessage,
                Context = conversationContext,
                Tools = RagToolCatalog.All
            });

            // Final answer → stop
            if (!llmResult.IsToolCall)
            {
                return new RagResponse
                {
                    Answer = llmResult.FinalAnswer ?? "No response",
                    CartSnapshot = await _contextBuilder.BuildAsync()
                };
            }

            // Tool execution
            var toolCall = llmResult.ToolCall!;
            var toolResult = await _dispatcher.DispatchAsync(
                toolCall.Name,
                toolCall.Arguments);

            // Inject tool result into context for next iteration
            conversationContext.Add(new
            {
                role = "tool",
                name = toolCall.Name,
                content = toolResult
            });
        }

        throw new InvalidOperationException(
            "AI exceeded maximum allowed tool calls");
    }
}
