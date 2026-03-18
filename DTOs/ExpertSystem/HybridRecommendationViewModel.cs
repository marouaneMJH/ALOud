namespace ALOud.DTOs.ExpertSystem
{
    /// <summary>
    /// View model for hybrid recommendations
    /// </summary>
    public class HybridRecommendationViewModel
    {
        /// <summary>
        /// The user's recommendation input
        /// </summary>
        public RecommendationDto? Recommendation { get; set; }

        /// <summary>
        /// The LLM generated response
        /// </summary>
        public string? LlmGeneratedResponse { get; set; }
    }
}