using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ALOud.Services.Perfume;
using ALOud.Services;
using ALOud.Controllers.Api;
using ALOud.DTOs.Perfumes;
using ViewModels;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for perfume operations (v1)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PerfumesController : BaseApiController
    {
        private readonly IPerfumeService _perfumeService;
        private readonly ICartService _cartService;
        private readonly ILogger<PerfumesController> _logger;

        /// <summary>
        /// Initializes a new instance of the PerfumesController class
        /// </summary>
        /// <param name="perfumeService">The perfume service</param>
        /// <param name="cartService">The cart service</param>
        /// <param name="logger">The logger</param>
        public PerfumesController(
            IPerfumeService perfumeService,
            ICartService cartService,
            ILogger<PerfumesController> logger)
        {
            _perfumeService = perfumeService;
            _cartService = cartService;
            _logger = logger;
        }

        #region Admin CRUD Endpoints

        /// <summary>
        /// Lists all perfumes for admin with optional filters (Authorize Required)
        /// </summary>
        /// <param name="query">Search term for perfume name or brand</param>
        /// <param name="brandId">Brand filter</param>
        /// <param name="familyId">Family filter</param>
        /// <param name="gender">Gender profile filter</param>
        /// <param name="pageIndex">Current page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated perfume list</returns>
        [HttpGet("admin")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListPerfumesAsync(
            [FromQuery] string? query,
            [FromQuery] Guid? brandId,
            [FromQuery] Guid? familyId,
            [FromQuery] string? gender,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 12)
        {
            _logger.LogInformation("Admin listing perfumes with filters: query={Query}, brandId={BrandId}, familyId={FamilyId}, gender={Gender}, pageIndex={PageIndex}, pageSize={PageSize}",
                query, brandId, familyId, gender, pageIndex, pageSize);

            var perfumes = await _perfumeService.GetAllPerfumesAsync(
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchTerm: query,
                brandId: brandId,
                familyId: familyId,
                genderProfile: gender);

            return SuccessResponse(perfumes);
        }

        /// <summary>
        /// Gets select list of all perfumes for dropdowns (Authorize Required)
        /// </summary>
        /// <returns>List of perfumes with id and name only</returns>
        [HttpGet("admin/select")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SelectPerfumesAsync()
        {
            _logger.LogInformation("Admin retrieving perfumes select list");

            var perfumes = await _perfumeService.GetAllPerfumesAsync(pageIndex: 1, pageSize: 10000);
            var selectList = perfumes.Items.Select(p => new { id = p.Id, name = p.Name }).ToList();

            return SuccessResponse(selectList);
        }

        /// <summary>
        /// Creates a new perfume (Authorize Required)
        /// </summary>
        /// <param name="dto">Perfume creation data transfer object</param>
        /// <returns>Created perfume ID</returns>
        [HttpPost("admin")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePerfumeAsync([FromBody] CreatePerfumeDto dto)
        {
            _logger.LogInformation("Admin creating perfume: {PerfumeName}", dto.Name);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationErrorResponse(new Dictionary<string, string[]>
                {
                    { "validation", errors.ToArray() }
                });
            }

            var perfumeId = await _perfumeService.CreatePerfumeAsync(dto);

            _logger.LogInformation("Perfume created successfully with ID: {PerfumeId}", perfumeId);
            return SuccessResponse(new { id = perfumeId, message = "Perfume created successfully" });
        }

        /// <summary>
        /// Gets a perfume for editing (Authorize Required)
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>Perfume edit data</returns>
        [HttpGet("admin/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPerfumeForEditAsync(Guid id)
        {
            _logger.LogInformation("Admin retrieving perfume for edit: {PerfumeId}", id);

            var perfume = await _perfumeService.GetPerfumeForEditAsync(id);

            if (perfume == null)
            {
                _logger.LogWarning("Perfume not found for edit: {PerfumeId}", id);
                return NotFoundResponse("Perfume not found");
            }

            return SuccessResponse(perfume);
        }

        /// <summary>
        /// Updates an existing perfume (Authorize Required)
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <param name="dto">Perfume update data transfer object</param>
        /// <returns>Success or error response</returns>
        [HttpPut("admin/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePerfumeAsync(Guid id, [FromBody] UpdatePerfumeDto dto)
        {
            _logger.LogInformation("Admin updating perfume: {PerfumeId}", id);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationErrorResponse(new Dictionary<string, string[]>
                {
                    { "validation", errors.ToArray() }
                });
            }

            if (id != dto.Id)
            {
                return ErrorResponse("Perfume ID in URL does not match ID in request body");
            }

            var success = await _perfumeService.UpdatePerfumeAsync(dto);

            if (!success)
            {
                _logger.LogWarning("Failed to update perfume: {PerfumeId}", id);
                return NotFoundResponse("Perfume not found");
            }

            _logger.LogInformation("Perfume updated successfully: {PerfumeId}", id);
            return SuccessResponse(new { id = id, message = "Perfume updated successfully" });
        }

        /// <summary>
        /// Deletes a perfume (Authorize Required)
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>Success or error response</returns>
        [HttpDelete("admin/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePerfumeAsync(Guid id)
        {
            _logger.LogInformation("Admin deleting perfume: {PerfumeId}", id);

            var success = await _perfumeService.DeletePerfumeAsync(id);

            if (!success)
            {
                _logger.LogWarning("Failed to delete perfume: {PerfumeId}", id);
                return NotFoundResponse("Perfume not found");
            }

            _logger.LogInformation("Perfume deleted successfully: {PerfumeId}", id);
            return SuccessResponse(new { id = id, message = "Perfume deleted successfully" });
        }

        /// <summary>
        /// Validates if a perfume name already exists (Authorize Required)
        /// </summary>
        /// <param name="request">Validation request containing perfume name and optional ID to exclude</param>
        /// <returns>Validation result</returns>
        [HttpPost("admin/validate-exists")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidatePerfumeExistsAsync([FromBody] ValidatePerfumeExistsRequest request)
        {
            _logger.LogInformation("Admin validating perfume existence: {Name}", request.Name);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ErrorResponse("Perfume name is required");
            }

            // Get all perfumes and check for duplicates (excluding current ID if provided)
            var perfumes = await _perfumeService.GetAllPerfumesAsync(pageIndex: 1, pageSize: 10000);
            var exists = perfumes.Items.Any(p => 
                p.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                (!request.ExcludeId.HasValue || p.Id != request.ExcludeId));

            return SuccessResponse(new { exists = exists });
        }

        #endregion

        #region Customer-Facing Endpoints

        /// <summary>
        /// Gets a paginated list of perfumes with optional filters
        /// </summary>
        /// <param name="query">Search term for perfume name or brand</param>
        /// <param name="brandId">Brand filter</param>
        /// <param name="familyId">Family filter</param>
        /// <param name="gender">Gender profile filter</param>
        /// <param name="pageIndex">Current page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated perfume list</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPerfumes(
            [FromQuery] string? query,
            [FromQuery] Guid? brandId,
            [FromQuery] Guid? familyId,
            [FromQuery] string? gender,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 12)
        {
            var perfumes = await _perfumeService.GetAllPerfumesAsync(
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchTerm: query,
                brandId: brandId,
                familyId: familyId,
                genderProfile: gender);

            return SuccessResponse(perfumes);
        }

        /// <summary>
        /// Gets detailed information for a specific perfume
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>Perfume details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPerfumeById(Guid id)
        {
            var perfume = await _perfumeService.GetPerfumeDetailsAsync(id);

            if (perfume == null)
            {
                return NotFoundResponse("Perfume not found");
            }

            return SuccessResponse(perfume);
        }

        /// <summary>
        /// Adds a perfume to the shopping cart
        /// </summary>
        /// <param name="request">Cart addition request</param>
        /// <returns>Success response</returns>
        [HttpPost("add-to-cart")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request.Quantity <= 0)
            {
                return ErrorResponse("Quantity must be greater than zero");
            }

            var perfume = await _perfumeService.GetPerfumeByIdAsync(request.PerfumeId);

            if (perfume == null)
            {
                return NotFoundResponse("Perfume not found");
            }

            if (perfume.StockQuantity < request.Quantity)
            {
                return ErrorResponse("Insufficient stock available");
            }

            await _cartService.AddToCartAsync(new CartItemVM
            {
                ProductId = perfume.Id,
                ProductName = perfume.Name,
                BrandName = perfume.BrandName,
                Price = perfume.Price,
                Quantity = request.Quantity,
                ImageUrl = perfume.ImageUrl ?? ""
            });

            return SuccessResponse(new { message = $"{perfume.Name} added to cart" });
        }

        #endregion
    }

    /// <summary>
    /// Request model for adding items to cart
    /// </summary>
    public class AddToCartRequest
    {
        /// <summary>
        /// The perfume identifier
        /// </summary>
        public Guid PerfumeId { get; set; }

        /// <summary>
        /// The quantity to add
        /// </summary>
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Request model for validating if a perfume exists
    /// </summary>
    public class ValidatePerfumeExistsRequest
    {
        /// <summary>
        /// The perfume name to validate
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional ID to exclude from validation (for updates)
        /// </summary>
        public Guid? ExcludeId { get; set; }
    }
}
