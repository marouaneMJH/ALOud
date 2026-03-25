using System.Security.Claims;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for user account management
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of the AccountController class
        /// </summary>
        /// <param name="userService">The user service</param>
        /// <param name="verificationService">The verification service</param>
        /// <param name="logger">The logger</param>
        public AccountController(
            IUserService userService,
            IVerificationService verificationService,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Displays the registration form
        /// </summary>
        /// <returns>Registration view</returns>
        [HttpGet("/Account/Register", Name = "MvcAccountRegisterGet")]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Processes user registration
        /// </summary>
        /// <param name="dto">The registration data</param>
        /// <returns>Redirect to verification or registration view with errors</returns>
        [HttpPost("/Account/Register", Name = "MvcAccountRegisterPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var user = await _userService.CreateUserAsync(dto);
            
            if (user == null)
            {
                AddModelError("An error occurred during registration");
                return View(dto);
            }

            await _verificationService.SendVerificationAsync(user);
            return RedirectToRoute("MvcAccountVerifyGet", new { email = user.Email });
        }

        /// <summary>
        /// Displays the login form
        /// </summary>
        /// <returns>Login view</returns>
        [HttpGet("/Account/Login", Name = "MvcAccountLoginGet")]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Processes user login
        /// </summary>
        /// <param name="dto">The login credentials</param>
        /// <returns>Redirect to home or login view with errors</returns>
        [HttpPost("/Account/Login", Name = "MvcAccountLoginPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var user = await _userService.AuthenticateAsync(dto);

            if (user == null)
            {
                var existing = await _userService.GetByEmailAsync(dto.Email);
                
                if (existing != null && !existing.IsEmailVerified)
                {
                    await _verificationService.SendVerificationAsync(existing);
                    return RedirectToRoute("MvcAccountVerifyGet", new { email = existing.Email });
                }

                AddModelError("Invalid credentials");
                return View(dto);
            }

            await SignInUserAsync(user.Id.ToString(), user.Email);
            return RedirectToAction("Index", "Perfume");
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        /// <returns>Redirect to login page</returns>
        [HttpPost("/Account/Logout", Name = "MvcAccountLogoutPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToRoute("MvcAccountLoginGet");
        }

        /// <summary>
        /// Displays the user profile
        /// </summary>
        /// <returns>Profile view</returns>
        [HttpGet("/Account/Profile", Name = "MvcAccountProfileGet")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            
            if (!userId.HasValue)
            {
                return RedirectToRoute("MvcAccountLoginGet");
            }

            var user = await _userService.GetByIdAsync(userId.Value);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        /// <summary>
        /// Processes email verification code
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="code">The verification code</param>
        /// <returns>Redirect to login or verification view with errors</returns>
        [HttpPost("/Account/Verify", Name = "MvcAccountVerifyPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string email, string code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                AddModelError("Email and code are required");
                SetViewDataEmail(email);
                return View();
            }

            var isValid = await _verificationService.VerifyCodeAsync(email, code);
            
            if (!isValid)
            {
                AddModelError("Invalid or expired code. Please verify the code or request a new one.");
                SetViewDataEmail(email);
                return View();
            }

            SetSuccessMessage("Email verified successfully. You can now login.");
            return RedirectToRoute("MvcAccountLoginGet");
        }

        /// <summary>
        /// Resends the verification email
        /// </summary>
        /// <param name="email">The email address</param>
        /// <returns>Redirect back to verification view with status message</returns>
        [HttpPost("/Account/ResendVerification", Name = "MvcAccountResendVerificationPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                AddModelError("Email is required");
                return RedirectToRoute("MvcAccountVerifyGet");
            }

            var success = await _verificationService.ResendVerificationAsync(email);
            
            if (success)
            {
                SetSuccessMessage("A new verification code has been sent to your email address.");
            }
            else
            {
                AddModelError("Unable to resend code. Please verify your email address is correct.");
            }

            return RedirectToRoute("MvcAccountVerifyGet", new { email });
        }

        #region Private Helper Methods

        /// <summary>
        /// Signs in a user by creating authentication claims and cookies
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="email">The user email</param>
        private async Task SignInUserAsync(string userId, string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
        }

        /// <summary>
        /// Gets the current authenticated user's ID
        /// </summary>
        /// <returns>The user ID if authenticated, null otherwise</returns>
        private Guid? GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
            {
                return null;
            }

            return userId;
        }

        /// <summary>
        /// Sets the email in ViewData for the view
        /// </summary>
        /// <param name="email">The email address</param>
        private void SetViewDataEmail(string email)
        {
            ViewData["Email"] = email;
        }

        /// <summary>
        /// Adds a model error message
        /// </summary>
        /// <param name="message">The error message</param>
        private void AddModelError(string message)
        {
            ModelState.AddModelError(string.Empty, message);
        }

        /// <summary>
        /// Sets a success message in TempData
        /// </summary>
        /// <param name="message">The success message</param>
        private void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        #endregion
    }
}
