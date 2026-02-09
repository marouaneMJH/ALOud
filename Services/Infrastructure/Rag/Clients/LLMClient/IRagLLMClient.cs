using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag.Clients;

public interface IRagLLMClient
{
    Task<RagLLMResult> ExecuteAsync(RagLLMRequest request);
}
