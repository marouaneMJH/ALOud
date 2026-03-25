using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ALOud.Models;
using ViewModels;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for home/static pages
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the HomeController class
        /// </summary>
        /// <param name="logger">The logger</param>
        public HomeController(ILogger<HomeController> logger)
        {
            // Logger parameter removed since it's not used
        }

        /// <summary>
        /// Displays the privacy policy page
        /// </summary>
        /// <returns>Privacy policy view</returns>
        [HttpGet]
        [Route("Home/Privacy", Name = "MvcHomePrivacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page
        /// </summary>
        /// <returns>Error view with error details</returns>
        [HttpGet]
        [Route("Home/Error", Name = "MvcHomeError")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
