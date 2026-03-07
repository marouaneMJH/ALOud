using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Rag.Clients;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Identity.Client;

public class HybridExpertSystemService : IHybridExpertSystemService
{
    private IEmbeddingClient  _embeddingClient;
    private IVectorSearchClient  _vectorSearchClient;
    private ILlmClient  _llmClient;

    public HybridExpertSystemService(
        IEmbeddingClient
        embeddingClient,
        IVectorSearchClient  vectorSearchClient,
        ILlmClient  llmClient
    )
    {
        _embeddingClient = embeddingClient;
        _vectorSearchClient = vectorSearchClient;
        _llmClient = llmClient;
    }


    public string Evaluate(Recommendation rec)
    {
        
        var products = GetTopKProducts(rec.Prefer,rec.Avoid,rec.Sillage,rec.Longevity);

        return GetLLMGeneratedRecommendation(products, rec.Reasons);
    }


    private RagRetrievedChunk  GetTopKProducts(HashSet<string> prefer,HashSet<string> avoid, string? sillage, string? longevity,int topK = 5)
    {
        return new RagRetrievedChunk();
    }


    private string GetLLMGeneratedRecommendation(RagRetrievedChunk products, List<string> reasons)
    {
        return "";
    }


}
