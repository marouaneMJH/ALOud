using System.Text.Json;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

public sealed class ExpertController : Controller
{
    private readonly IHybridExpertSystemService _hybridExpert;
    private readonly ILogger<ExpertController> _logger;

    public ExpertController(
        IHybridExpertSystemService hybridExpert,
        ILogger<ExpertController> logger)
    {
        _hybridExpert = hybridExpert;
        _logger = logger;
    }

    [HttpPost]
    [Route("expert/hybrid-recommendation")]
    public async Task<IActionResult> HybridRecommendation([FromForm] string recommendationJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recommendationJson))
                return BadRequest("Invalid recommendation data");

            var recommendation = JsonSerializer.Deserialize<RecommendationDto>(recommendationJson);
            if (recommendation == null)
                return BadRequest("Failed to parse recommendation data");

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

            return View("HybridRecommendation", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing hybrid recommendation");
            return BadRequest($"Error processing recommendation: {ex.Message}");
        }
    }
}

public class RecommendationDto
{
    public List<string>? Prefer { get; set; }
    public List<string>? Avoid { get; set; }
    public string? Sillage { get; set; }
    public string? Longevity { get; set; }
    public List<string>? Reasons { get; set; }
    public string? Result { get; set; }
}

public class HybridRecommendationViewModel
{
    public RecommendationDto? Recommendation { get; set; }
    public string? LlmGeneratedResponse { get; set; }
}
