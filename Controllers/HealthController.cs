using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly ILogger<RagChatController> _logger;

    public HealthController(
        ILogger<RagChatController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Health()
    {
        return Ok($"{{\n\"status\": \"ok\",\n\t\"timestamp\": \"{DateTime.Now.ToShortDateString()}\",\n\t\"service\": \"aloud-store\"\n}}");
    }
}
