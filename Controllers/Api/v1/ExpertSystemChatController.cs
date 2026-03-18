using ALOud.DTOs.ExpertSystem;
using ALOud.DTOs.Rag;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Rag;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for expert system evaluations and recommendations
    /// </summary>
    [ApiController]
    [Route("api/v1/ai/expert-system")]
    public class ExpertSystemChatController : ControllerBase
    {
        private readonly IExpertSystemService _expertService;
        private readonly IHybridExpertSystemService _hybridExpertService;
        private readonly ILogger<ExpertSystemChatController> _logger;

        /// <summary>
        /// Initializes a new instance of the ExpertSystemChatController class
        /// </summary>
        /// <param name="expertService">The expert system service</param>
        /// <param name="hybridExpertService">The hybrid expert system service</param>
        /// <param name="logger">The logger</param>
        public ExpertSystemChatController(
            IExpertSystemService expertService,
            IHybridExpertSystemService hybridExpertService,
            ILogger<ExpertSystemChatController> logger)
        {
            _expertService = expertService;
            _hybridExpertService = hybridExpertService;
            _logger = logger;
        }

        /// <summary>
        /// Tests the expert system evaluation with a user profile
        /// </summary>
        /// <param name="userProfileDto">The user profile for evaluation</param>
        /// <returns>Expert system evaluation result</returns>
        [HttpPost("test")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Test([FromBody] UserProfileDto userProfileDto)
        {
            if (userProfileDto == null)
                return BadRequest(new { error = "User profile is required." });

            try
            {
                var result = _expertService.Evaluate(userProfileDto);
                
                // Also get LLM explanation for complete response
                string llmResult = string.Empty;
                try
                {
                    llmResult = await _hybridExpertService.EvaluateAsync(result).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate LLM explanation, returning expert system result only");
                }
                
                // Return complete recommendation with LLM result
                return Ok(new
                {
                    prefer = result.Prefer,
                    avoid = result.Avoid,
                    sillage = result.Sillage,
                    longevity = result.Longevity,
                    reasons = result.Reasons,
                    result = llmResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating user profile");
                return StatusCode(500, new { error = "Expert system evaluation failed." });
            }
        }

        /// <summary>
        /// Evaluates a recommendation using the hybrid expert system
        /// </summary>
        /// <param name="recommendation">The recommendation to evaluate</param>
        /// <returns>Hybrid system evaluation result</returns>
        [HttpPost("evaluate")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Evaluate([FromBody] RecommendationDto recommendation)
        {
            if (recommendation == null)
                return BadRequest(new { error = "Recommendation data is required." });

            try
            {
                // Convert DTO to domain model
                var rec = new Recommendation();

                if (recommendation.Prefer != null)
                {
                    foreach (var item in recommendation.Prefer)
                    {
                        rec.Prefer.Add(item);
                    }
                }

                if (recommendation.Avoid != null)
                {
                    foreach (var item in recommendation.Avoid)
                    {
                        rec.Avoid.Add(item);
                    }
                }

                if (!string.IsNullOrWhiteSpace(recommendation.Sillage))
                    rec.Sillage = recommendation.Sillage;

                if (!string.IsNullOrWhiteSpace(recommendation.Longevity))
                    rec.Longevity = recommendation.Longevity;

                if (recommendation.Reasons != null)
                {
                    rec.Reasons.AddRange(recommendation.Reasons);
                }

                var result = await _hybridExpertService.EvaluateAsync(rec);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in hybrid expert system evaluation");
                return StatusCode(500, new { error = "Hybrid expert system evaluation failed." });
            }
        }
    }

    /// <summary>
    /// DTO for expert system recommendations
    /// </summary>
    public class RecommendationDto
    {
        /// <summary>Preferred characteristics</summary>
        public List<string>? Prefer { get; set; }

        /// <summary>Characteristics to avoid</summary>
        public List<string>? Avoid { get; set; }

        /// <summary>Preferred sillage</summary>
        public string? Sillage { get; set; }

        /// <summary>Preferred longevity</summary>
        public string? Longevity { get; set; }

        /// <summary>Recommendation reasons</summary>
        public List<string>? Reasons { get; set; }

        /// <summary>LLM generated result</summary>
        public string? Result { get; set; }
    }
}
