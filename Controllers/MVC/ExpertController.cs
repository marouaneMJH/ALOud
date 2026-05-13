using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for expert system recommendations
    /// </summary>
    [Route("Expert", Name = "MvcExpertPrefix")]
    public class ExpertController : Controller
    {
        private readonly IHybridExpertSystemService _hybridExpert;
        private readonly ILogger<ExpertController> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
        [Route("HybridRecommendation", Name = "MvcExpertHybridGet")]
        public IActionResult HybridRecommendation()
        {
            return View(new HybridRecommendationViewModel { Recommendation = new RecommendationDto() });
        }

        /// <summary>
        /// Processes hybrid recommendation request
        /// </summary>
        /// <param name="recommendationJson">The recommendation data as JSON string</param>
        /// <returns>View with recommendation result</returns>
        [HttpPost]
        [Route("HybridRecommendation", Name = "MvcExpertHybridPost")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1024 * 1024)] // 1MB limit
        public async Task<IActionResult> HybridRecommendation([FromForm] string recommendationJson)
        {
            if (string.IsNullOrWhiteSpace(recommendationJson))
            {
                SetErrorMessage("Invalid recommendation data: JSON cannot be empty");
                return View(new HybridRecommendationViewModel { Recommendation = new RecommendationDto() });
            }

            try
            {
                _logger.LogInformation("[Recommendation POST] Received JSON: {JsonSubstring}...", recommendationJson.Substring(0, Math.Min(200, recommendationJson.Length)));
                
                var recommendation = JsonSerializer.Deserialize<RecommendationDto>(recommendationJson, JsonOptions);
                
                if (recommendation == null)
                {
                    _logger.LogError("[Recommendation POST] Deserialization returned null");
                    SetErrorMessage("Failed to parse recommendation data: Invalid JSON format");
                    return View(new HybridRecommendationViewModel { Recommendation = new RecommendationDto() });
                }
                
                _logger.LogInformation("[Recommendation POST] Deserialized successfully. Prefer: {PreferCount}, Avoid: {AvoidCount}", 
                    recommendation.Prefer?.Count ?? 0, recommendation.Avoid?.Count ?? 0);

                // Validate DTO
                if ((recommendation.Prefer == null || recommendation.Prefer.Count == 0) &&
                    (recommendation.Avoid == null || recommendation.Avoid.Count == 0) &&
                    string.IsNullOrWhiteSpace(recommendation.Sillage) &&
                    string.IsNullOrWhiteSpace(recommendation.Longevity))
                {
                    SetErrorMessage("Please provide at least one search criterion (prefer, avoid, sillage, or longevity)");
                    return View(new HybridRecommendationViewModel { Recommendation = recommendation });
                }

                // Convert DTO to domain model efficiently
                var rec = new Recommendation();

                // Use HashSet operations for better performance
                if (recommendation.Prefer?.Count > 0)
                {
                    rec.Prefer.UnionWith(recommendation.Prefer.Where(p => !string.IsNullOrWhiteSpace(p)));
                }

                if (recommendation.Avoid?.Count > 0)
                {
                    rec.Avoid.UnionWith(recommendation.Avoid.Where(a => !string.IsNullOrWhiteSpace(a)));
                }

                if (!string.IsNullOrWhiteSpace(recommendation.Sillage))
                    rec.Sillage = recommendation.Sillage.Trim();

                if (!string.IsNullOrWhiteSpace(recommendation.Longevity))
                    rec.Longevity = recommendation.Longevity.Trim();

                if (recommendation.Reasons?.Count > 0)
                {
                    rec.Reasons.AddRange(recommendation.Reasons.Where(r => !string.IsNullOrWhiteSpace(r)));
                }

                // Generate LLM recommendation
                var result = await _hybridExpert.EvaluateAsync(rec).ConfigureAwait(false);

                var viewModel = new HybridRecommendationViewModel
                {
                    Recommendation = recommendation,
                    LlmGeneratedResponse = result.LlmResponse,
                    Products = result.Products
                };

                SetSuccessMessage("Recommendation generated successfully");
                return View(viewModel);
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Request was cancelled during hybrid recommendation processing");
                SetErrorMessage("Request was cancelled. Please try again.");
                return View(new HybridRecommendationViewModel { Recommendation = new RecommendationDto() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing hybrid recommendation");
                SetErrorMessage("An error occurred while processing your recommendation. Please try again.");
                return View(new HybridRecommendationViewModel { Recommendation = new RecommendationDto() });
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
}
