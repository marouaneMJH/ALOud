using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class RagCartService
{
    private const int MaxToolCalls = 8;

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
        var cartContext = await _contextBuilder.BuildAsync();
        var conversationHistory = new List<ConversationTurn>();

        for (var step = 0; step < MaxToolCalls; step++)
        {
            var llmResult = await _llm.ExecuteAsync(new RagLLMRequest
            {
                SystemPrompt = SystemPrompts.CartAssistant,
                UserMessage = userMessage,
                Context = cartContext,
                ConversationHistory = conversationHistory,
                Tools = RagToolCatalog.All
            });

            // Log the step
            Console.WriteLine($"[RAG Step {step + 1}/{MaxToolCalls}] IsToolCall: {llmResult.IsToolCall}, Tool: {llmResult.ToolCall?.Name}");

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

            // Add model's function call to history
            conversationHistory.Add(new ConversationTurn
            {
                Role = "assistant",
                FunctionCallName = toolCall.Name,
                FunctionCallArgs = toolCall.Arguments
            });

            var toolResult = await _dispatcher.DispatchAsync(
                toolCall.Name,
                toolCall.Arguments);

            // Add function response to history
            conversationHistory.Add(new ConversationTurn
            {
                Role = "tool",
                FunctionName = toolCall.Name,
                FunctionResponse = toolResult
            });
        }

        throw new InvalidOperationException(
            "AI exceeded maximum allowed tool calls");
    }
}
