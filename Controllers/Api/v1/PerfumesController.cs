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
}
