using ALOud.DTOs.Rag;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

[ApiController]
[Route("api/ai/expert-system")]
public sealed class ExpertSystemChatController : ControllerBase
{
    private readonly IExpertSystemService _expert;
    private readonly ILogger<RagChatController> _logger;

    public ExpertSystemChatController(
        IExpertSystemService expert,
        ILogger<RagChatController> logger)
    {
        _expert = expert;
        _logger = logger;
    }




    [HttpGet("expert-test")]
    public IActionResult Test()
    {
        var profile = new UserProfile
        {
            Climate = EClimate.Hot,
            Occasion = EOccasion.Gym,
            SkinType = ESkinType.Oily,
            Compliment = EComplimentDesire.Neutral,
        };

        var result = _expert.Evaluate(profile);
        return Ok(result);
    }

}
