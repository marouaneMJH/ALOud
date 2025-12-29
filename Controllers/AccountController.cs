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

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
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
        public async Task<IActionResult> Login(LoginDto? dto)
        {
            _logger.LogInformation("=== LOGIN ATTEMPT START ===");

            // Log raw form data
            _logger.LogInformation("=== RAW FORM DATA ===");
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation($"Form[{key}] = '{Request.Form[key]}'");
            }

            // Try manual binding as fallback
            if (dto == null || (string.IsNullOrEmpty(dto.Email) && string.IsNullOrEmpty(dto.Password)))
            {
                _logger.LogInformation("DTO binding failed, trying manual binding...");
                dto = new LoginDto
                {
                    Email = Request.Form["Email"].ToString(),
                    Password = Request.Form["Password"].ToString()
                };
                _logger.LogInformation($"Manual binding - Email: '{dto.Email}', Password: {(!string.IsNullOrEmpty(dto.Password) ? "[PROVIDED]" : "[EMPTY]")}");
            }

            _logger.LogInformation($"Email received: '{dto?.Email ?? "null"}'");
            _logger.LogInformation($"Password received: {(!string.IsNullOrEmpty(dto?.Password) ? "[PROVIDED]" : "[EMPTY/NULL]")}");
            _logger.LogInformation($"DTO is null: {dto == null}");

            if (dto == null)
            {
                _logger.LogWarning("DTO is still null after manual binding attempt");
                return View();
            }

            // Re-validate after manual binding
            if (dto.Email != Request.Form["Email"].ToString() || dto.Password != Request.Form["Password"].ToString())
            {
                ModelState.Clear();
                TryValidateModel(dto);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation Error: {error.ErrorMessage}");
                }
                return View(dto);
            }

            _logger.LogInformation("ModelState is valid, attempting authentication");

            try
            {
                var user = await _userService.AuthenticateAsync(dto);

                if (user == null)
                {
                    _logger.LogWarning($"Authentication failed for email: {dto.Email}");
                    ModelState.AddModelError(string.Empty, "Invalid credentials");
                    return View(dto);
                }

                _logger.LogInformation($"User authenticated successfully: {user.Email}, ID: {user.Id}");

                await SignInUser(user.Id.ToString(), user.Email);

                _logger.LogInformation("User signed in successfully, redirecting to Home");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login process");
                ModelState.AddModelError(string.Empty, "An error occurred during login");
                return View(dto);
            }
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
            _logger.LogInformation($"=== SIGNIN PROCESS START === UserId: {userId}, Email: {email}");

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email)
                };

                _logger.LogInformation($"Claims created: NameIdentifier={userId}, Email={email}");

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                _logger.LogInformation($"ClaimsIdentity created with scheme: {CookieAuthenticationDefaults.AuthenticationScheme}");

                var principal = new ClaimsPrincipal(identity);
                _logger.LogInformation("ClaimsPrincipal created");

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                _logger.LogInformation("HttpContext.SignInAsync completed successfully");
                _logger.LogInformation($"User.Identity.IsAuthenticated: {HttpContext.User.Identity?.IsAuthenticated ?? false}");
                _logger.LogInformation($"User.Identity.Name: {HttpContext.User.Identity?.Name ?? "null"}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during SignInUser process");
                throw;
            }
        }
    }
}
