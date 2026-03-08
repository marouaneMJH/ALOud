using System.Security.Claims;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for AI-powered shopping cart operations (RAG)
    /// </summary>
    [ApiController]
    [Route("api/v1/ai/cart")]
    public class AiCartController : ControllerBase
    {
        private readonly RagCartService _ragCartService;
        private readonly IChatOrchestratorService _chatOrchestratorService;
        private readonly ILogger<AiCartController> _logger;

        /// <summary>
        /// Initializes a new instance of the AiCartController class
        /// </summary>
        /// <param name="ragCartService">The RAG cart service</param>
        /// <param name="chatOrchestratorService">The chat orchestrator service</param>
        /// <param name="logger">The logger</param>
        public AiCartController(
            RagCartService ragCartService,
            IChatOrchestratorService chatOrchestratorService,
            ILogger<AiCartController> logger)
        {
            _ragCartService = ragCartService;
            _chatOrchestratorService = chatOrchestratorService;
            _logger = logger;
        }

        /// <summary>
        /// Handles AI cart operations
        /// </summary>
        /// <param name="request">The cart operation request</param>
        /// <returns>AI cart response</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RagResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RagResponse>> Handle([FromBody] RagRequest? request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message is required." });

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

        /// <summary>
        /// Initiates a chat conversation for cart operations
        /// </summary>
        /// <param name="request">The chat request</param>
        /// <returns>Chat response</returns>
        [HttpPost("chat")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Chat([FromBody] RagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { error = "Message is required." });

            _logger.LogDebug("AI Cart chat message: {Message}", request.Message);

            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                {
                    return Unauthorized(new { error = "User must be authenticated." });
                }
                
                var response = await _chatOrchestratorService.HandleAsync(userId, request.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI cart chat");
                return StatusCode(500, new { error = "An error occurred. Please try again." });
            }
        }
    }
}
