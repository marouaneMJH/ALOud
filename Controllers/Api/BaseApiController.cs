using ALOud.Common;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.Api
{
    /// <summary>
    /// Base controller for API endpoints with standardized response handling
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Converts a Result to an appropriate HTTP response
        /// </summary>
        /// <typeparam name="T">The type of data in the result</typeparam>
        /// <param name="result">The result to convert</param>
        /// <returns>An IActionResult with appropriate status code and response body</returns>
        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(new { success = true, data = result.Data });
            }

            if (result.ValidationErrors != null)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    error = result.Error,
                    validationErrors = result.ValidationErrors 
                });
            }

            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new { success = false, error = result.Error });
            }

            return BadRequest(new { success = false, error = result.Error });
        }

        /// <summary>
        /// Converts a Result without data to an appropriate HTTP response
        /// </summary>
        /// <param name="result">The result to convert</param>
        /// <returns>An IActionResult with appropriate status code and response body</returns>
        protected IActionResult ToActionResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }

            if (result.ValidationErrors != null)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    error = result.Error,
                    validationErrors = result.ValidationErrors 
                });
            }

            return BadRequest(new { success = false, error = result.Error });
        }

        /// <summary>
        /// Creates a standardized success response with data
        /// </summary>
        /// <typeparam name="T">The type of data</typeparam>
        /// <param name="data">The data to return</param>
        /// <returns>An OK result with standardized JSON structure</returns>
        protected IActionResult SuccessResponse<T>(T data)
        {
            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="statusCode">The HTTP status code (defaults to 400)</param>
        /// <returns>An error result with standardized JSON structure</returns>
        protected IActionResult ErrorResponse(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new { success = false, error = message });
        }

        /// <summary>
        /// Creates a standardized not found response
        /// </summary>
        /// <param name="message">Optional custom message (defaults to "Resource not found")</param>
        /// <returns>A NotFound result with standardized JSON structure</returns>
        protected IActionResult NotFoundResponse(string message = "Resource not found")
        {
            return NotFound(new { success = false, error = message });
        }

        /// <summary>
        /// Creates a standardized validation error response
        /// </summary>
        /// <param name="errors">Dictionary of field names to error messages</param>
        /// <returns>A BadRequest result with validation errors</returns>
        protected IActionResult ValidationErrorResponse(Dictionary<string, string[]> errors)
        {
            return BadRequest(new 
            { 
                success = false, 
                error = "Validation failed",
                validationErrors = errors 
            });
        }
    }
}
