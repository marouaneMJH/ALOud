using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for service health checks
    /// </summary>
    [ApiController]
    [Route("api/v1/health")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the HealthController class
        /// </summary>
        /// <param name="logger">The logger</param>
        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the health status of the service
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            try
            {
                return Ok(new
                {
                    status = "ok",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    service = "aloud-store"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health status");
                return StatusCode(500, new
                {
                    status = "error",
                    timestamp = DateTime.Now.ToShortDateString(),
                    message = "Service health check failed"
                });
            }
        }
    }
}
