using Microsoft.AspNetCore.Authorization;
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
        /// Displays the RAG-powered shopping assistant chat interface (anonymous)
        /// </summary>
        /// <returns>Chat view</returns>
        [HttpGet]
        [Route("Chat", Name = "MvcChatIndex")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the AI-powered cart assistant chat interface (requires login).
        /// Backed by /api/v1/ai/cart/chat which uses IChatOrchestratorService with user context.
        /// </summary>
        /// <returns>AiCart view</returns>
        [HttpGet]
        [Route("Chat/AiCart", Name = "MvcChatAiCart")]
        [Authorize]
        public IActionResult AiCart()
        {
            return View();
        }
    }
}

