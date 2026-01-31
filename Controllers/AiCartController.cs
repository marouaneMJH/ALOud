using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

[ApiController]
[Route("api/ai/cart")]
public sealed class AiCartController : ControllerBase
{
    private readonly RagCartService _ragCartService;
    private readonly ILogger<AiCartController> _logger;

    public AiCartController(
        RagCartService ragCartService,
        ILogger<AiCartController> logger)
    {
        _ragCartService = ragCartService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<RagResponse>> Handle([FromBody] RagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        _logger.LogInformation("AI Cart request: {Message}", request.Message);

        try
        {
            var response = await _ragCartService.HandleAsync(request.Message);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("limite de requêtes"))
        {
            _logger.LogWarning("Rate limit hit: {Message}", ex.Message);
            return StatusCode(429, new RagResponse
            {
                Answer = ex.Message,
                CartSnapshot = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI cart request");
            return StatusCode(500, new RagResponse
            {
                Answer = "An error occurred. Please try again.",
                CartSnapshot = null
            });
        }
    }
}
