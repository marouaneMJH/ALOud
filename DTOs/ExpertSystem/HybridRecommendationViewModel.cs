namespace ALOud.DTOs.ExpertSystem
{
    public class HybridRecommendationViewModel
    {
        public RecommendationDto? Recommendation { get; set; }
        public string? LlmGeneratedResponse { get; set; }
        public IReadOnlyList<RecommendedPerfumeDto> Products { get; set; } = [];
    }
}