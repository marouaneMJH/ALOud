using System.Security.Claims;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for user address management (AJAX-driven)
    /// </summary>
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IUserAddressService _addressService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(
            IUserAddressService addressService,
            ILogger<AddressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the standalone "My Addresses" management page
        /// </summary>
        [HttpGet]
        [Route("Address", Name = "MvcAddressIndex")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                var addresses = await _addressService.GetUserAddressesAsync(userId);
                return View(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading address management page");
                TempData["Error"] = "Failed to load addresses";
                return RedirectToAction("Index", "Perfume");
            }
        }

        /// <summary>
        /// Returns the address list partial view
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var userId = GetUserId();
                var addresses = await _addressService.GetUserAddressesAsync(userId);
                return PartialView("_AddressList", addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address list partial");
                return StatusCode(500, "Error loading addresses");
            }
        }

        /// <summary>
        /// Returns the user's default address as JSON (for AJAX use by checkout, etc.)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDefault()
        {
            try
            {
                var userId = GetUserId();
                var address = await _addressService.GetDefaultAddressAsync(userId);
                if (address == null)
                    return Json(new { success = false, message = "No default address" });
                return Json(new { success = true, data = address });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default address");
                return Json(new { success = false, message = "Error loading default address" });
            }
        }

        /// <summary>
        /// Returns the create address partial form
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddressForm", new CreateAddressDto());
        }

        /// <summary>
        /// Processes address creation via AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAddressDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddressForm", dto);
            }

            try
            {
                var userId = GetUserId();
                await _addressService.CreateAddressAsync(userId, dto);
                return Json(new { success = true, message = "Address added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return Json(new { success = false, message = "Failed to add address" });
            }
        }

        /// <summary>
        /// Returns the edit address partial form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var address = await _addressService.GetUserAddressAsync(userId, id);
                if (address == null) return NotFound();

                var dto = new UpdateAddressDto
                {
                    Id = address.Id,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    PhoneNumber = address.PhoneNumber,
                    AddressType = address.AddressType,
                    IsDefault = address.IsDefault
                };

                return PartialView("_AddressForm", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address for edit: {AddressId}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Processes address update via AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateAddressDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddressForm", dto);
            }

            try
            {
                var userId = GetUserId();
                await _addressService.UpdateAddressAsync(userId, dto);
                return Json(new { success = true, message = "Address updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address: {AddressId}", dto.Id);
                return Json(new { success = false, message = "Failed to update address" });
            }
        }

        /// <summary>
        /// Sets an address as default via AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var success = await _addressService.SetDefaultAddressAsync(userId, id);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address: {AddressId}", id);
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Deletes an address via AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var success = await _addressService.DeleteAddressAsync(userId, id);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address: {AddressId}", id);
                return Json(new { success = false });
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException();
        }
    }
}
