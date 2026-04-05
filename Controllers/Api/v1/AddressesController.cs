using ALOud.Controllers.Api;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for user address management (v1)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class AddressesController : BaseApiController
    {
        private readonly IUserAddressService _addressService;
        private readonly ILogger<AddressesController> _logger;

        /// <summary>
        /// Initializes a new instance of the AddressesController class
        /// </summary>
        /// <param name="addressService">The address service</param>
        /// <param name="logger">The logger</param>
        public AddressesController(
            IUserAddressService addressService,
            ILogger<AddressesController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all addresses for the current user
        /// </summary>
        /// <returns>List of user addresses</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var addresses = await _addressService.GetUserAddressesAsync(userId);
                return SuccessResponse(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for user");
                return ErrorResponse("Failed to retrieve addresses", 500);
            }
        }

        /// <summary>
        /// Gets a specific address by ID
        /// </summary>
        /// <param name="id">The address ID</param>
        /// <returns>The address details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAddress(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Validate ownership
                if (!await _addressService.ValidateAddressOwnershipAsync(userId, id))
                {
                    return NotFoundResponse("Address not found");
                }

                var address = await _addressService.GetUserAddressAsync(userId, id);
                if (address == null)
                {
                    return NotFoundResponse("Address not found");
                }

                return SuccessResponse(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId} for user", id);
                return ErrorResponse("Failed to retrieve address", 500);
            }
        }

        /// <summary>
        /// Gets the user's default address
        /// </summary>
        /// <returns>The default address</returns>
        [HttpGet("default")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDefaultAddress()
        {
            try
            {
                var userId = GetCurrentUserId();
                var address = await _addressService.GetDefaultAddressAsync(userId);
                
                if (address == null)
                {
                    return NotFoundResponse("No default address found");
                }

                return SuccessResponse(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default address for user");
                return ErrorResponse("Failed to retrieve default address", 500);
            }
        }

        /// <summary>
        /// Creates a new address for the current user
        /// </summary>
        /// <param name="dto">The address creation data</param>
        /// <returns>The created address</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                var userId = GetCurrentUserId();
                var address = await _addressService.CreateAddressAsync(userId, dto);
                
                return CreatedAtAction(
                    nameof(GetAddress),
                    new { id = address.Id },
                    new { success = true, data = address }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user");
                return ErrorResponse("Failed to create address", 500);
            }
        }

        /// <summary>
        /// Updates an existing address
        /// </summary>
        /// <param name="id">The address ID</param>
        /// <param name="dto">The address update data</param>
        /// <returns>The updated address</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressDto dto)
        {
            if (id != dto.Id)
            {
                return ErrorResponse("Address ID in URL does not match request body");
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                var userId = GetCurrentUserId();
                
                // Validate ownership
                if (!await _addressService.ValidateAddressOwnershipAsync(userId, id))
                {
                    return NotFoundResponse("Address not found");
                }

                var address = await _addressService.UpdateAddressAsync(userId, dto);
                return SuccessResponse(address);
            }
            catch (ArgumentException)
            {
                return NotFoundResponse("Address not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for user", id);
                return ErrorResponse("Failed to update address", 500);
            }
        }

        /// <summary>
        /// Deletes an address
        /// </summary>
        /// <param name="id">The address ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Validate ownership
                if (!await _addressService.ValidateAddressOwnershipAsync(userId, id))
                {
                    return NotFoundResponse("Address not found");
                }

                var success = await _addressService.DeleteAddressAsync(userId, id);
                if (!success)
                {
                    return NotFoundResponse("Address not found");
                }

                return SuccessResponse("Address deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user", id);
                return ErrorResponse("Failed to delete address", 500);
            }
        }

        /// <summary>
        /// Sets an address as the default address
        /// </summary>
        /// <param name="id">The address ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/set-default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetDefaultAddress(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Validate ownership
                if (!await _addressService.ValidateAddressOwnershipAsync(userId, id))
                {
                    return NotFoundResponse("Address not found");
                }

                var success = await _addressService.SetDefaultAddressAsync(userId, id);
                if (!success)
                {
                    return NotFoundResponse("Address not found");
                }

                return SuccessResponse("Default address set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for user", id);
                return ErrorResponse("Failed to set default address", 500);
            }
        }

        /// <summary>
        /// Gets the current user's ID from the JWT claims
        /// </summary>
        /// <returns>The current user's ID</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            return userId;
        }
    }
}