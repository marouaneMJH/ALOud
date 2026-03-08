using Microsoft.AspNetCore.Mvc;
using ALOud.Services;
using ALOud.Services.Perfume;
using ALOud.Services.Brand;
using ALOud.Services.Family;
using ALOud.DTOs.Perfumes;
using ViewModels;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for perfume catalog and product pages
    /// </summary>
    public class PerfumeController : Controller
    {
        private readonly IPerfumeService _perfumeService;
        private readonly IBrandService _brandService;
        private readonly IFamilyService _familyService;
        private readonly ICartService _cartService;
        private readonly ILogger<PerfumeController> _logger;

        /// <summary>
        /// Initializes a new instance of the PerfumeController class
        /// </summary>
        /// <param name="perfumeService">The perfume service</param>
        /// <param name="brandService">The brand service</param>
        /// <param name="familyService">The family service</param>
        /// <param name="cartService">The cart service</param>
        /// <param name="logger">The logger</param>
        public PerfumeController(
            IPerfumeService perfumeService,
            IBrandService brandService,
            IFamilyService familyService,
            ICartService cartService,
            ILogger<PerfumeController> logger)
        {
            _perfumeService = perfumeService;
            _brandService = brandService;
            _familyService = familyService;
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the perfume catalog with filtering and pagination
        /// </summary>
        /// <param name="query">Search term for perfume name or brand</param>
        /// <param name="brandId">Brand filter</param>
        /// <param name="familyId">Family filter</param>
        /// <param name="gender">Gender profile filter</param>
        /// <param name="pageIndex">Current page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Catalog view with perfumes</returns>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? query, 
            Guid? brandId, 
            Guid? familyId, 
            string? gender, 
            int pageIndex = 1, 
            int pageSize = 12)
        {
            var perfumesResult = await _perfumeService.GetAllPerfumesAsync(
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchTerm: query,
                brandId: brandId,
                familyId: familyId,
                genderProfile: gender);

            var viewModel = await BuildCatalogViewModelAsync(
                perfumesResult, 
                query, 
                brandId, 
                familyId, 
                gender);

            return View(viewModel);
        }

        /// <summary>
        /// Displays detailed information for a specific perfume
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>Details view for the perfume</returns>
        [HttpGet("Perfume/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var perfume = await _perfumeService.GetPerfumeDetailsAsync(id);

            if (perfume == null)
            {
                SetErrorMessage("Perfume not found");
                return NotFound();
            }

            return View(perfume);
        }

        /// <summary>
        /// Adds a perfume to the shopping cart
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <param name="quantity">The quantity to add</param>
        /// <returns>Redirect to perfume details page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid id, int quantity = 1)
        {
            if (quantity <= 0)
            {
                SetErrorMessage("Quantity must be greater than zero");
                return RedirectToAction(nameof(Details), new { id });
            }

            var perfume = await _perfumeService.GetPerfumeByIdAsync(id);

            if (perfume == null)
            {
                SetErrorMessage("Perfume not found");
                return NotFound();
            }

            if (perfume.StockQuantity < quantity)
            {
                SetErrorMessage("Insufficient stock available");
                return RedirectToAction(nameof(Details), new { id });
            }

            await _cartService.AddToCartAsync(new CartItemVM
            {
                ProductId = perfume.Id,
                ProductName = perfume.Name,
                BrandName = perfume.BrandName,
                Price = perfume.Price,
                Quantity = quantity,
                ImageUrl = perfume.ImageUrl ?? ""
            });

            SetSuccessMessage($"{perfume.Name} added to cart");
            return RedirectToAction(nameof(Details), new { id });
        }

        #region Private Helper Methods

        /// <summary>
        /// Builds the catalog view model with perfumes and filter options
        /// </summary>
        /// <param name="perfumes">Paginated perfume list</param>
        /// <param name="query">Current search query</param>
        /// <param name="brandId">Current brand filter</param>
        /// <param name="familyId">Current family filter</param>
        /// <param name="gender">Current gender filter</param>
        /// <returns>Complete catalog view model</returns>
        private async Task<PerfumeCatalogViewModel> BuildCatalogViewModelAsync(
            PaginatedList<PerfumeDto> perfumes,
            string? query,
            Guid? brandId,
            Guid? familyId,
            string? gender)
        {
            var brandsForSelect = await _brandService.GetAllBrandsForSelectAsync();
            var familiesForSelect = await _familyService.GetAllFamiliesForSelectAsync();

            return new PerfumeCatalogViewModel
            {
                Perfumes = perfumes,
                Brands = brandsForSelect,
                Families = familiesForSelect,
                SearchQuery = query,
                SelectedBrandId = brandId,
                SelectedFamilyId = familyId,
                SelectedGender = gender
            };
        }

        /// <summary>
        /// Sets a success message in TempData for display to the user
        /// </summary>
        /// <param name="message">The success message</param>
        private void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Sets an error message in TempData for display to the user
        /// </summary>
        /// <param name="message">The error message</param>
        private void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        #endregion
    }
}
