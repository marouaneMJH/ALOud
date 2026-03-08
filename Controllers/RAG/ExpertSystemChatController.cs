using System.Threading.Tasks;
using ALOud.DTOs.ExpertSystem;
using ALOud.DTOs.Rag;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

[ApiController]
[Route("api/ai/expert-system")]
public sealed class ExpertSystemChatController : ControllerBase
{
    private readonly IExpertSystemService _expert;
    private readonly IHybridExpertSystemService _hybridExpert;
    private readonly ILogger<RagChatController> _logger;

    public ExpertSystemChatController(
        IExpertSystemService expert,
        IHybridExpertSystemService hybridExpert,
        ILogger<RagChatController> logger)
    {
        _expert = expert;
        _hybridExpert = hybridExpert;
        _logger = logger;
    }


    [HttpPost("expert-test")]
    public IActionResult Test([FromBody] UserProfileDto userProfileDto)
    {

        var result = _expert.Evaluate(userProfileDto);
        return Ok(result);
    }


}
