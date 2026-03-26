using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services;
using ALOud.Common;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// REST API Controller for Admin Dashboard and Configuration
    /// Converts MVC AdminController to REST endpoints
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/admin")]
    public class AdminController : BaseApiController
    {
        private readonly IDashboardService _dashboardService;
        private readonly LLMConfigService _llmConfigService;
        private readonly ILogger<AdminController> _logger;
        
        private static readonly string[] ProviderIdRequiredError = { "Provider ID cannot be empty" };

        /// <summary>
        /// Initializes a new instance of the AdminController class
        /// </summary>
        public AdminController(
            IDashboardService dashboardService,
            LLMConfigService llmConfigService,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _llmConfigService = llmConfigService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/v1/admin/dashboard
        /// Retrieves dashboard statistics and KPIs
        /// Replaces: MVC AdminController.Index() [HttpGet]
        /// </summary>
        /// <returns>Dashboard statistics with success/error response</returns>
        /// <response code="200">Dashboard statistics retrieved successfully</response>
        /// <response code="401">Unauthorized - Authentication required</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                _logger.LogInformation("Dashboard statistics retrieved successfully");
                return SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard statistics");
                return ErrorResponse("Failed to load dashboard statistics", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/llm-config
        /// Retrieves available LLM providers and current configuration
        /// Replaces: MVC AdminController.LLMConfig() [HttpGet]
        /// </summary>
        /// <returns>List of available LLM providers</returns>
        /// <response code="200">LLM providers retrieved successfully</response>
        /// <response code="401">Unauthorized - Authentication required</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("llm-config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetLLMConfig()
        {
            try
            {
                var providers = _llmConfigService.GetAvailableProviders();
                _logger.LogInformation("LLM providers retrieved successfully");
                return SuccessResponse(new { providers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading LLM providers");
                return ErrorResponse("Failed to load LLM configuration", 500);
            }
        }

        /// <summary>
        /// POST /api/v1/admin/llm-config/switch-provider
        /// Switches the active LLM provider
        /// Replaces: MVC AdminController.SwitchProvider(string providerId) [HttpPost]
        /// </summary>
        /// <param name="request">Request containing the provider ID to switch to</param>
        /// <returns>Success or error response</returns>
        /// <response code="200">Provider switched successfully</response>
        /// <response code="400">Bad request - Invalid provider ID</response>
        /// <response code="401">Unauthorized - Authentication required</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("llm-config/switch-provider")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SwitchLLMProvider([FromBody] SwitchProviderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ProviderId))
            {
                _logger.LogWarning("Switch provider request with invalid provider ID");
                return BadRequest(new 
                { 
                    success = false, 
                    error = "Provider ID is required",
                    validationErrors = new { ProviderId = ProviderIdRequiredError }
                });
            }

            try
            {
                var success = await _llmConfigService.SwitchProvider(request.ProviderId);

                if (!success)
                {
                    _logger.LogWarning("Failed to switch LLM provider: {ProviderId}", request.ProviderId);
                    return BadRequest(new
                    {
                        success = false,
                        error = "Failed to switch provider. Please check the logs or provider configuration."
                    });
                }

                _logger.LogInformation("Successfully switched to LLM provider: {ProviderId}", request.ProviderId);
                return SuccessResponse(new
                {
                    message = $"Successfully switched to {request.ProviderId}",
                    note = "The change will take effect on the next request"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching LLM provider: {ProviderId}", request.ProviderId);
                return ErrorResponse("An error occurred while switching provider", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/expert-system
        /// Retrieves expert system configuration
        /// Replaces: MVC AdminController.ExpertSystem() [HttpGet]
        /// </summary>
        /// <returns>Expert system configuration</returns>
        /// <response code="200">Expert system configuration retrieved successfully</response>
        /// <response code="401">Unauthorized - Authentication required</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("expert-system")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetExpertSystemConfig()
        {
            try
            {
                // TODO: Implement expert system configuration retrieval
                // This would fetch the current expert system rules, configuration, etc.
                _logger.LogInformation("Expert system configuration retrieved");
                return SuccessResponse(new
                {
                    message = "Expert system configuration",
                    config = new { }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expert system configuration");
                return ErrorResponse("Failed to load expert system configuration", 500);
            }
        }
    }

    /// <summary>
    /// Request model for switching LLM provider
    /// </summary>
    public class SwitchProviderRequest
    {
        /// <summary>
        /// The ID of the LLM provider to switch to
        /// </summary>
        public string ProviderId { get; set; } = string.Empty;
    }
}
