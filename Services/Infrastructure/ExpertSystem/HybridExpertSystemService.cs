using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Rag.Clients;
using ALOud.Services.Rag.Models;
using System.Text;

public class HybridExpertSystemService : IHybridExpertSystemService
{
    private readonly IPayloadSearchClient _payloadSearchClient;
    private readonly IRagLLMClient _llmClient;

    public HybridExpertSystemService(
        IPayloadSearchClient payloadSearchClient,
        IRagLLMClient llmClient
    )
    {
        _payloadSearchClient = payloadSearchClient;
        _llmClient = llmClient;
    }


    public async Task<string> EvaluateAsync(
        Recommendation rec,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rec);

        var products = await GetTopKProducts(
            rec.Prefer,
            rec.Avoid,
            rec.Sillage,
            rec.Longevity,
            topK: 5,
            cancellationToken: cancellationToken);

        return await GetLLMGeneratedRecommendationAsync(
            products,
            rec.Reasons,
            cancellationToken);
    }


    private async Task<IReadOnlyList<RagRetrievedChunk>> GetTopKProducts(
        HashSet<string> prefer,
        HashSet<string> avoid,
        string? sillage,
        string? longevity,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildQdrantFilter(prefer, avoid, sillage, longevity);

        var results = await _payloadSearchClient.SearchByFilterAsync(
            topK,
            filter: filter,
            cancellationToken: cancellationToken);

        var uniqueProducts = results
            .GroupBy(GetSourceKey)
            .Select(g => g.First())
            .ToList();

        return uniqueProducts;
    }

    private static QdrantFilter BuildQdrantFilter(
        HashSet<string> prefer,
        HashSet<string> avoid,
        string? sillage,
        string? longevity)
    {
        var filter = new QdrantFilter();

        foreach (var term in prefer.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            filter.Must.Add(new FilterCondition
            {
                Key = "content",
                Value = term.Trim()
            });
        }

        foreach (var term in avoid.Where(a => !string.IsNullOrWhiteSpace(a)))
        {
            filter.MustNot.Add(new FilterCondition
            {
                Key = "content",
                Value = term.Trim()
            });
        }

        if (!string.IsNullOrWhiteSpace(sillage))
        {
            filter.Must.Add(new FilterCondition
            {
                Key = "content",
                Value = sillage.Trim()
            });
        }

        if (!string.IsNullOrWhiteSpace(longevity))
        {
            filter.Must.Add(new FilterCondition
            {
                Key = "content",
                Value = longevity.Trim()
            });
        }

        return filter;
    }

    private async Task<string> GetLLMGeneratedRecommendationAsync(
        IReadOnlyList<RagRetrievedChunk> products,
        List<string> reasons,
        CancellationToken cancellationToken = default)
    {
        var productContext = BuildProductContext(products);
        var reasonContext = reasons.Count == 0
            ? "No explicit reasons provided by user."
            : string.Join("\n", reasons.Select(r => $"- {r}"));

        var result = await _llmClient.ExecuteAsync(new RagLLMRequest
        {
            SystemPrompt = "You are a perfume recommendation expert. Recommend only from provided products. Respect avoid constraints and explain why each product fits the user profile.",
            UserMessage = "Generate a concise recommendation response with top products, why they fit, and a short caution for any trade-off.",
            Context = new
            {
                reasons = reasonContext,
                products = productContext
            }
        });

        if (!string.IsNullOrWhiteSpace(result.FinalAnswer))
            return result.FinalAnswer;

        return BuildFallbackResponse(products);
    }

    private static string GetSourceKey(RagRetrievedChunk chunk)
    {
        if (chunk.Metadata.TryGetValue("sourceId", out var sourceId) && sourceId != null)
            return sourceId.ToString() ?? chunk.Id;

        return chunk.Id;
    }

    private static List<object> BuildProductContext(IReadOnlyList<RagRetrievedChunk> products)
    {
        return products.Select(product => new
        {
            id = product.Id,
            sourceId = GetSourceKey(product),
            name = ExtractField(product.Content, "Perfume Name:"),
            brand = ExtractField(product.Content, "Brand:"),
            content = product.Content,
            metadata = product.Metadata
        } as object).ToList();
    }

    private static string ExtractField(string content, string label)
    {
        var lines = content.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var value = lines
            .FirstOrDefault(line => line.StartsWith(label, StringComparison.OrdinalIgnoreCase));

        if (value == null)
            return string.Empty;

        return value.Substring(label.Length).Trim();
    }

    private static string BuildFallbackResponse(IReadOnlyList<RagRetrievedChunk> products)
    {
        if (products.Count == 0)
            return "I couldn’t find products that match your constraints. Try relaxing the avoid/preference criteria.";

        var sb = new StringBuilder();
        sb.AppendLine("Recommended products:");

        foreach (var product in products)
        {
            var name = ExtractField(product.Content, "Perfume Name:");
            var brand = ExtractField(product.Content, "Brand:");
            sb.AppendLine($"- {name} by {brand}");
        }

        return sb.ToString().Trim();
    }
}
