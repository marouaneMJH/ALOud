using ALOud.Models;
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.Services.Infrastructure.Rag.Agent;
using ALOud.Services.Infrastructure.Rag.ChatAPI;

namespace ALOud.Services.Rag;

public sealed class RagAnswerService : IRagAnswerService
{
    private readonly IChatOrchestratorService _chatOrchestrator;
    private readonly ILogger<RagAnswerService> _logger;

    public RagAnswerService(
        IChatOrchestratorService chatOrchestrator,
        ILogger<RagAnswerService> logger)
    {
        _chatOrchestrator = chatOrchestrator;
        _logger = logger;
    }

    public async Task<RagDebugResult> AnswerAsync(
        Guid userId,
        string question,
        RagAnswerOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question cannot be empty", nameof(question));

        _logger.LogInformation(
            "RAG answer requested | Debug={Debug} | TopK={TopK}",
            options.Debug,
            options.TopK
        );

        //  Debug path
        if (options.Debug)
        {
            return await _chatOrchestrator.HandleWithDebugAsync(
                userId,
                question,
                cancellationToken
            );
        }

        //  Normal path
        var answer = await _chatOrchestrator.HandleAsync(
            userId,
            question,
            cancellationToken
        );

        return new RagDebugResult
        {
            Answer = answer,
            RetrievedChunks = Array.Empty<RagRetrievedChunk>()
        };
    }
}
