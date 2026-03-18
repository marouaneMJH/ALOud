using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.ExpertSystem
{
    /// <summary>
    /// DTO for expert system recommendations
    /// </summary>
    public class RecommendationDto
    {
        /// <summary>
        /// Preferred characteristics
        /// </summary>
        [MaxLength(50, ErrorMessage = "Maximum 50 preferred characteristics allowed")]
        public List<string>? Prefer { get; set; }

        /// <summary>
        /// Characteristics to avoid
        /// </summary>
        [MaxLength(50, ErrorMessage = "Maximum 50 avoid characteristics allowed")]
        public List<string>? Avoid { get; set; }

        /// <summary>
        /// Preferred sillage level
        /// </summary>
        [MaxLength(100, ErrorMessage = "Sillage description cannot exceed 100 characters")]
        public string? Sillage { get; set; }

        /// <summary>
        /// Preferred longevity
        /// </summary>
        [MaxLength(100, ErrorMessage = "Longevity description cannot exceed 100 characters")]
        public string? Longevity { get; set; }

        /// <summary>
        /// Reasons for recommendation
        /// </summary>
        [MaxLength(10, ErrorMessage = "Maximum 10 reasons allowed")]
        public List<string>? Reasons { get; set; }

        /// <summary>
        /// LLM generated result
        /// </summary>
        [MaxLength(5000, ErrorMessage = "Result cannot exceed 5000 characters")]
        public string? Result { get; set; }
    }
}