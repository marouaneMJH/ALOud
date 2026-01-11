using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public interface IRagLLMClient
{
    Task<RagResponse> ExecuteAsync(RagRequest request);
}
