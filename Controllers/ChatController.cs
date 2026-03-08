using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        // GET: /Chat
        public IActionResult Index()
        {
            return View();
        }
    }
}
