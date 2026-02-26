using ALOud.DTOs.Rag;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

[ApiController]
[Route("api/ai/rag")]
public sealed class RagChatController : ControllerBase
{
    private readonly IChatOrchestratorService _chatOrchestrator;
    private readonly ILogger<RagChatController> _logger;

    public RagChatController(
        IChatOrchestratorService chatOrchestrator,
        ILogger<RagChatController> logger)
    {
        _chatOrchestrator = chatOrchestrator;
        _logger = logger;
    }


    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] RagChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var userId = Guid.NewGuid(); // fix: temp until auth

        try
        {
            //  DEBUG MODE
            if (request.Debug)
            {
                var debugResult = await _chatOrchestrator
                    .HandleWithDebugAsync(userId, request.Message);

                return Ok(new
                {
                    answer = debugResult.Answer,
                    debug = new
                    {
                        retrievedChunks = debugResult.RetrievedChunks.Select(c => new
                        {
                            c.Id,
                            c.Score,
                            preview = c.Content.Length > 200
                                ? c.Content[..200] + "..."
                                : c.Content
                        })
                    }
                });
            }

            // NORMAL MODE
            var answer = await _chatOrchestrator
                .HandleAsync(userId, request.Message);

            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG chat failed");
            return StatusCode(500, new { error = "RAG chat failed" });
        }
    }

}
