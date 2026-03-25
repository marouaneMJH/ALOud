using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for chat interface
    /// </summary>
    public class ChatController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the ChatController class
        /// </summary>
        /// <param name="logger">The logger</param>
        public ChatController(ILogger<ChatController> logger)
        {
            // Logger parameter removed since it's not used
        }

        /// <summary>
        /// Displays the chat interface
        /// </summary>
        /// <returns>Chat view</returns>
        [HttpGet]
        [Route("Chat", Name = "MvcChatIndex")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
