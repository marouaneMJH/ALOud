using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public interface IRagAnswerService
{
    Task<RagDebugResult> AnswerAsync(
        Guid userId,
        string question,
        RagAnswerOptions options,
        CancellationToken cancellationToken = default
    );
}
