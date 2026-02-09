// Orchestrate the entire RAG runtime flow from user message -> final answer.
/*
User message
   ↓
ChatOrchestratorService
   ├─ QueryEmbeddingService
   ├─ RetrievalService
   ├─ RagCartService (live data)
   ├─ ContextBuilderService
   └─ LlmGenerationService
   ↓
Final answer

*/
using System.Text.Json;
using ALOud.Services.Cart;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;

public class ChatOrchestratorService : IChatOrchestratorService
{
    private readonly IQueryEmbeddingService _queryEmbeddingService;
    private readonly IRetrievalService _retrievalService;
    private readonly IContextBuilderService _contextBuilderService;
    private readonly ILlmGenerationService _llmGenerationService;
    private readonly CartContextBuilder _cartContextBuilder;


    public ChatOrchestratorService(
        IQueryEmbeddingService queryEmbeddingService,
        IRetrievalService retrievalService,
        IContextBuilderService contextBuilderService,
        ILlmGenerationService llmGenerationService
       )
    {
        _queryEmbeddingService = queryEmbeddingService;
        _retrievalService = retrievalService;
        _contextBuilderService = contextBuilderService;
        _llmGenerationService = llmGenerationService;
    }

    public async Task<string> HandleAsync(
        Guid userId,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        // 1. Embed user query
        var queryVector = await _queryEmbeddingService.EmbedAsync(
            message,
            cancellationToken
        );

        // 2. Retrieve static knowledge
        var retrievedChunks = await _retrievalService.RetrieveAsync(
            queryVector,
            topK: 5,
            cancellationToken: cancellationToken
        );

        // 3. Fetch live cart context

        var liveCartContext = $"""
            CURRENT CART:
            {JsonSerializer.Serialize(await _cartContextBuilder.BuildAsync(userId))}
        """;

        // 4. Build final context
        var contextPayload = _contextBuilderService.Build(
            userQuery: message,
            retrievedChunks: retrievedChunks,
            liveContext: liveCartContext
        );

        // 5. Generate final answer
        return await _llmGenerationService.GenerateAsync(
            contextPayload,
            message,
            cancellationToken
        );
    }


    public async Task<RagDebugResult> HandleWithDebugAsync(
    Guid userId,
    string message,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        // 1. Embed query
        var queryVector = await _queryEmbeddingService
            .EmbedAsync(message, cancellationToken);

        // 2. Retrieve
        var retrievedChunks = await _retrievalService
            .RetrieveAsync(queryVector, topK: 5, cancellationToken: cancellationToken);

        // 3. Live cart context
        var liveCartContext = await BuildLiveContextAsync(userId);

        // 4. Build context
        var contextPayload = _contextBuilderService.Build(
            userQuery: message,
            retrievedChunks: retrievedChunks,
            liveContext: liveCartContext
        );

        // 5. Generate answer
        var answer = await _llmGenerationService.GenerateAsync(
            contextPayload,
            message,
            cancellationToken
        );

        return new RagDebugResult
        {
            Answer = answer,
            RetrievedChunks = retrievedChunks
        };
    }


    async private Task<String> BuildLiveContextAsync(Guid userId)
    {
        return $"""
            CURRENT CART:
            {JsonSerializer.Serialize(await _cartContextBuilder.BuildAsync(userId))}
        """;
    }
}
