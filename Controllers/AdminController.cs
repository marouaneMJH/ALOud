using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ALOud.Services;

namespace ALOud.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly LLMConfigService _llmConfigService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IDashboardService dashboardService,
            LLMConfigService llmConfigService,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _llmConfigService = llmConfigService;
            _logger = logger;
        }

        // Dashboard with KPIs
        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return View(stats);
        }

        // =====================================================
        // LLM CONFIGURATION
        // =====================================================

        public IActionResult LLMConfig()
        {
            var providers = _llmConfigService.GetAvailableProviders();
            return View(providers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchProvider(string providerId)
        {
            if (string.IsNullOrEmpty(providerId))
            {
                TempData["Error"] = "Provider ID is required";
                return RedirectToAction(nameof(LLMConfig));
            }

            var success = await _llmConfigService.SwitchProvider(providerId);

            if (success)
            {
                TempData["Success"] = $"Successfully switched to {providerId}. The change will take effect on the next request.";
            }
            else
            {
                TempData["Error"] = "Failed to switch provider. Please check the logs.";
            }

            return RedirectToAction(nameof(LLMConfig));
        }
    }
}
