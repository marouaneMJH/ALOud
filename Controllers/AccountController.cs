using System.Security.Claims;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;

        }

        // ======================
        // REGISTER
        // ======================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(dto);
            }

            try
            {
                await _userService.CreateUserAsync(dto);
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // ======================
        // LOGIN
        // ======================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {

            Console.WriteLine(dto.Email);
            Console.WriteLine(dto.Password);

            if (!ModelState.IsValid)
            {
                // TODO remove The debug inst
                Console.WriteLine("Login class is not valid");
                Console.WriteLine(ModelState.Values);
                return View(dto);
            }


            var user = await _userService.AuthenticateAsync(dto);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(dto);
            }

            await SignInUser(user.Id.ToString(), user.Email);

            return RedirectToAction("Index", "Home");
        }

        // ======================
        // LOGOUT
        // ======================

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ======================
        // PRIVATE
        // ======================

        private async Task SignInUser(string userId, string email)
        {
            Console.WriteLine("email:", email);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

        }
    }
}
