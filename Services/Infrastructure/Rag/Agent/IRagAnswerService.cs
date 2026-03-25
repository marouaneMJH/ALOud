using ALOud.Services.Infrastructure.Rag.Models;

using ALOud.Services.Infrastructure.Rag.Agent;
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
