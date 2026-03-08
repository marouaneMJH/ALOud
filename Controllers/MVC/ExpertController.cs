using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Rag.Models;
using ALOud.DTOs.ExpertSystem;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for expert system recommendations
    /// </summary>
    public class ExpertController : Controller
    {
        private readonly IHybridExpertSystemService _hybridExpert;
        private readonly ILogger<ExpertController> _logger;

        /// <summary>
        /// Initializes a new instance of the ExpertController class
        /// </summary>
        /// <param name="hybridExpert">The hybrid expert system service</param>
        /// <param name="logger">The logger</param>
        public ExpertController(
            IHybridExpertSystemService hybridExpert,
            ILogger<ExpertController> logger)
        {
            _hybridExpert = hybridExpert;
            _logger = logger;
        }

        /// <summary>
        /// Displays the hybrid recommendation page
        /// </summary>
        /// <returns>Hybrid recommendation form view</returns>
        [HttpGet]
        [Route("Expert/HybridRecommendation", Name = "MvcExpertHybridGet")]
        public IActionResult HybridRecommendation()
        {
            return View(new RecommendationDto());
        }

        /// <summary>
        /// Processes hybrid recommendation request
        /// </summary>
        /// <param name="recommendationJson">The recommendation data as JSON string</param>
        /// <returns>View with recommendation result</returns>
        [HttpPost]
        [Route("Expert/HybridRecommendation", Name = "MvcExpertHybridPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HybridRecommendation([FromForm] string recommendationJson)
        {
            if (string.IsNullOrWhiteSpace(recommendationJson))
            {
                SetErrorMessage("Invalid recommendation data");
                return View(new RecommendationDto());
            }

            try
            {
                var recommendation = JsonSerializer.Deserialize<RecommendationDto>(recommendationJson);
                if (recommendation == null)
                {
                    SetErrorMessage("Failed to parse recommendation data");
                    return View(new RecommendationDto());
                }

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

                // Generate LLM recommendation
                var result = await _hybridExpert.EvaluateAsync(rec);

                var viewModel = new HybridRecommendationViewModel
                {
                    Recommendation = recommendation,
                    LlmGeneratedResponse = result
                };

                SetSuccessMessage("Recommendation generated successfully");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing hybrid recommendation");
                SetErrorMessage($"Error processing recommendation: {ex.Message}");
                return View(new RecommendationDto());
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Sets a success message in TempData
        /// </summary>
        /// <param name="message">The success message</param>
        private void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Sets an error message in TempData
        /// </summary>
        /// <param name="message">The error message</param>
        private void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        #endregion
    }

    /// <summary>
    /// DTO for expert system recommendations
    /// </summary>
    public class RecommendationDto
    {
        /// <summary>
        /// Preferred characteristics
        /// </summary>
        public List<string>? Prefer { get; set; }

        /// <summary>
        /// Characteristics to avoid
        /// </summary>
        public List<string>? Avoid { get; set; }

        /// <summary>
        /// Preferred sillage level
        /// </summary>
        public string? Sillage { get; set; }

        /// <summary>
        /// Preferred longevity
        /// </summary>
        public string? Longevity { get; set; }

        /// <summary>
        /// Reasons for recommendation
        /// </summary>
        public List<string>? Reasons { get; set; }

        /// <summary>
        /// LLM generated result
        /// </summary>
        public string? Result { get; set; }
    }

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
