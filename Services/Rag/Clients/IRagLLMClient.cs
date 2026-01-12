using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public interface IRagLLMClient
{
    Task<RagLLMResult> ExecuteAsync(RagLLMRequest request);
}
