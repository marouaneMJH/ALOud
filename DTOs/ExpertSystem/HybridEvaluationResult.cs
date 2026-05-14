namespace ALOud.DTOs.ExpertSystem;

public class HybridEvaluationResult
{
    public string LlmResponse { get; init; } = string.Empty;
    public IReadOnlyList<RecommendedPerfumeDto> Products { get; init; } = [];
}
