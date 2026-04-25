using ALOud.DTOs.Rag;
using ALOud.Services.Rag;
using ALOud.Services.Infrastructure.Rag.Models;
using Microsoft.AspNetCore.Mvc;
using ALOud.Services.Infrastructure.Rag.ChatAPI;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for RAG-based chat operations
    /// </summary>
    [ApiController]
    [Route("api/v1/ai/rag")]
    public class RagChatController : ControllerBase
    {
        private readonly IChatOrchestratorService _chatOrchestrator;
        private readonly ILogger<RagChatController> _logger;

        /// <summary>
        /// Initializes a new instance of the RagChatController class
        /// </summary>
        /// <param name="chatOrchestrator">The chat orchestrator service</param>
        /// <param name="logger">The logger</param>
        public RagChatController(
            IChatOrchestratorService chatOrchestrator,
            ILogger<RagChatController> logger)
        {
            _chatOrchestrator = chatOrchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Processes a chat message using RAG with optional debug mode
        /// </summary>
        /// <param name="request">The chat request</param>
        /// <returns>Chat response with optional debug information</returns>
        [HttpPost("chat")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Chat([FromBody] RagChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { error = "Message is required." });

            var userId = Guid.NewGuid(); // TODO: Get from authenticated user context

            try
            {
                // DEBUG MODE - includes retrieved chunks for analysis
                if (request.Debug)
                {
                    _logger.LogInformation("RAG chat debug mode: {Message}", request.Message);

                    var debugResult = await _chatOrchestrator
                        .HandleWithDebugAsync(userId, request.Message);

                    return Ok(new
                    {
                        success = true,
                        answer = debugResult.Answer,
                        debug = new
                        {
                            retrievedChunks = debugResult.RetrievedChunks != null ? 
                                debugResult.RetrievedChunks.Select(c => new
                                {
                                    c.Id,
                                    c.Score,
                                    preview = c.Content.Length > 200
                                        ? c.Content[..200] + "..."
                                        : c.Content
                                }).ToList() : 
                                (object)new List<object>()
                        }
                    });
                }

                // NORMAL MODE - standard response
                _logger.LogInformation("RAG chat message: {Message}", request.Message);

                var answer = await _chatOrchestrator
                    .HandleAsync(userId, request.Message);

                return Ok(new
                {
                    success = true,
                    answer = answer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RAG chat: {Message}", request.Message);
                return StatusCode(500, new
                {
                    success = false,
                    error = "RAG chat processing failed. Please try again."
                });
            }
        }
    }
}
