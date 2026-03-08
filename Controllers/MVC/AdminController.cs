using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for admin dashboard and configuration
    /// </summary>
    [Authorize]
    [Route("Admin", Name = "MvcAdminPrefix")]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly LLMConfigService _llmConfigService;
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Initializes a new instance of the AdminController class
        /// </summary>
        /// <param name="dashboardService">The dashboard service</param>
        /// <param name="llmConfigService">The LLM configuration service</param>
        /// <param name="logger">The logger</param>
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
        /// Displays the admin dashboard with KPIs
        /// </summary>
        /// <returns>Dashboard view with statistics</returns>
        [HttpGet]
        [Route("", Name = "MvcAdminIndex")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard statistics");
                SetErrorMessage("Failed to load dashboard");
                return View();
            }
        }

        /// <summary>
        /// Displays the LLM configuration page
        /// </summary>
        /// <returns>LLM configuration view</returns>
        [HttpGet]
        [Route("LLMConfig", Name = "MvcAdminLLMConfig")]
        public IActionResult LLMConfig()
        {
            try
            {
                var providers = _llmConfigService.GetAvailableProviders();
                return View(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading LLM providers");
                SetErrorMessage("Failed to load LLM configuration");
                return View();
            }
        }

        /// <summary>
        /// Switches the active LLM provider
        /// </summary>
        /// <param name="providerId">The provider identifier</param>
        /// <returns>Redirect to LLM config page</returns>
        [HttpPost]
        [Route("SwitchProvider", Name = "MvcAdminSwitchProvider")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchProvider(string providerId)
        {
            if (string.IsNullOrEmpty(providerId))
            {
                SetErrorMessage("Provider ID is required");
                return RedirectToRoute("MvcAdminLLMConfig");
            }

            try
            {
                var success = await _llmConfigService.SwitchProvider(providerId);

                if (success)
                {
                    SetSuccessMessage($"Successfully switched to {providerId}. The change will take effect on the next request.");
                }
                else
                {
                    SetErrorMessage("Failed to switch provider. Please check the logs.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching LLM provider: {ProviderId}", providerId);
                SetErrorMessage("An error occurred while switching provider");
            }

            return RedirectToRoute("MvcAdminLLMConfig");
        }

        /// <summary>
        /// Displays the expert system configuration page
        /// </summary>
        /// <returns>Expert system view</returns>
        [HttpGet]
        [Route("ExpertSystem", Name = "MvcAdminExpertSystem")]
        public IActionResult ExpertSystem()
        {
            try
            {
                // Pass enum values to the view for dropdowns
                // This ensures UI always matches server-side enums
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expert system configuration");
                SetErrorMessage("Failed to load expert system configuration");
                return View();
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Sets a success message in TempData
        /// </summary>
        /// <param name="message">The success message</param>
        private void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Sets an error message in TempData
        /// </summary>
        /// <param name="message">The error message</param>
        private void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        #endregion
    }
}
