namespace ALOud.Services.Rag;

public sealed class RagCartService
{
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

    public async Task<string> HandleAsync(string userMessage)
    {
        var context = await _contextBuilder.BuildAsync();

        // 1. Send message + context to LLM
        // 2. LLM decides tool or final answer
        // 3. If tool → dispatch
        // 4. Return final answer

        throw new NotImplementedException("Next step: system prompt + GROQ integration");
    }
}
