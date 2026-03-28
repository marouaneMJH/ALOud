using ALOud.DTOs;
using ALOud.Services;
using ALOud.Controllers.Api;
using ALOud.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for account operations (v1)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
     public class AccountController : BaseApiController
     {
         private readonly IUserService _userService;
         private readonly IVerificationService _verificationService;
         private readonly IJwtTokenService _jwtTokenService;

         /// <summary>
         /// Initializes a new instance of the AccountController class
         /// </summary>
         /// <param name="userService">The user service</param>
         /// <param name="verificationService">The verification service</param>
         /// <param name="jwtTokenService">The JWT token service</param>
         /// <param name="logger">The logger</param>
         public AccountController(
             IUserService userService,
             IVerificationService verificationService,
             IJwtTokenService jwtTokenService,
             ILogger<AccountController> logger)
         {
             _userService = userService;
             _verificationService = verificationService;
             _jwtTokenService = jwtTokenService;
         }

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="dto">The registration data</param>
        /// <returns>Created user information</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(
                    ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    ));
            }

            var user = await _userService.CreateUserAsync(dto);

            if (user == null)
            {
                return ErrorResponse("An error occurred during registration");
            }

            await _verificationService.SendVerificationAsync(user);

            return SuccessResponse(new
            {
                message = "Registration successful. Please check your email for verification code.",
                email = user.Email
            });
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="dto">The login credentials</param>
        /// <returns>Authentication result with user information</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(
                    ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    ));
            }

            var user = await _userService.AuthenticateAsync(dto);

            if (user == null)
            {
                var existing = await _userService.GetByEmailAsync(dto.Email);

                if (existing != null && !existing.IsEmailVerified)
                {
                    await _verificationService.SendVerificationAsync(existing);
                    
                    return ErrorResponse(
                        "Email not verified. A new verification code has been sent.",
                        StatusCodes.Status403Forbidden);
                }

                return ErrorResponse("Invalid credentials", StatusCodes.Status401Unauthorized);
            }

            return SuccessResponse(new
            {
                message = "Login successful",
                // TODO: Add admin role check
                token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, user.Email == "admin@admin.com"),
                user = new
                {
                    id = user.Id,
                    email = user.Email, 
                    firstName = user.FirstName,
                    lastName = user.LastName
                }
            });
        }

        /// <summary>
        /// Verifies a user's email with the provided code
        /// </summary>
        /// <param name="request">The verification request containing email and code</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Verify([FromBody] VerifyEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
            {
                return ErrorResponse("Email and code are required");
            }

            var isValid = await _verificationService.VerifyCodeAsync(request.Email, request.Code);

            if (!isValid)
            {
                return ErrorResponse("Invalid or expired verification code");
            }

            return SuccessResponse(new
            {
                message = "Email verified successfully"
            });
        }

        /// <summary>
        /// Resends the verification code to the user's email
        /// </summary>
        /// <param name="request">Request containing the email address</param>
        /// <returns>Confirmation of resend</returns>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return ErrorResponse("Email is required");
            }

            var success = await _verificationService.ResendVerificationAsync(request.Email);

            if (!success)
            {
                return ErrorResponse("Unable to resend verification. Please check your email address.");
            }

            return SuccessResponse(new
            {
                message = "Verification code sent successfully"
            });
        }

        /// <summary>
        /// Gets the current user's profile
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return ErrorResponse("Invalid user ID", StatusCodes.Status401Unauthorized);
            }

            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFoundResponse("User not found");
            }

            return SuccessResponse(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                isEmailVerified = user.IsEmailVerified
            });
        }
    }

    /// <summary>
    /// Request model for email verification
    /// </summary>
    public class VerifyEmailRequest
    {
        /// <summary>
        /// The email address to verify
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The verification code
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for resending verification code
    /// </summary>
    public class ResendVerificationRequest
    {
        /// <summary>
        /// The email address
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}
