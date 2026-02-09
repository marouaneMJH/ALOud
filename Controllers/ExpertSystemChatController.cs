using ALOud.DTOs.ExpertSystem;
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


    [HttpPost("expert-test")]
    public IActionResult Test([FromBody] UserProfileDto userProfileDto)
    {

        var result = _expert.Evaluate(userProfileDto);
        return Ok(result);
    }

}
