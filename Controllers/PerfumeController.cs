using Microsoft.AspNetCore.Mvc;
using ALOud.Services;
using ALOud.Services.Perfume;
using ALOud.Services.Brand;
using ALOud.Services.Family;
using ALOud.DTOs.Perfumes;
using ViewModels;

namespace ALOud.Controllers
{
    public class PerfumeController : Controller
    {
        private readonly IPerfumeService _perfumeService;
        private readonly IBrandService _brandService;
        private readonly IFamilyService _familyService;
        private readonly CartService _cartService;
        private readonly ILogger<PerfumeController> _logger;

        public PerfumeController(
            IPerfumeService perfumeService,
            IBrandService brandService,
            IFamilyService familyService,
            CartService cartService,
            ILogger<PerfumeController> logger)
        {
            _perfumeService = perfumeService;
            _brandService = brandService;
            _familyService = familyService;
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Perfume
        // Catalog listing with filters and search
        [HttpGet]
        public async Task<IActionResult> Index(string? query, Guid? brandId, Guid? familyId, string? gender, int pageIndex = 1, int pageSize = 12)
        {
            var perfumesResult = await _perfumeService.GetAllPerfumesAsync(
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchTerm: query,
                brandId: brandId,
                familyId: familyId,
                genderProfile: gender);

            // Fetch brands and families for filter UI
            var brandsForSelect = await _brandService.GetAllBrandsForSelectAsync();
            var familiesForSelect = await _familyService.GetAllFamiliesForSelectAsync();

            ViewBag.Brands = brandsForSelect;
            ViewBag.Families = familiesForSelect;
            ViewBag.SearchQuery = query;
            ViewBag.SelectedBrandId = brandId;
            ViewBag.SelectedFamilyId = familyId;
            ViewBag.SelectedGender = gender;

            return View(perfumesResult);
        }

        // GET: /Perfume/Details/{id}
        // Product details page
        [HttpGet("Perfume/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var perfume = await _perfumeService.GetPerfumeDetailsAsync(id);

            if (perfume == null)
                return RedirectToAction(nameof(Index));

            return View(perfume);
        }

        // POST: /Perfume/AddToCart
        // Add perfume to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid id, int quantity = 1)
        {
            var perfume = await _perfumeService.GetPerfumeByIdAsync(id);

            if (perfume == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Index));
            }

            if (perfume.StockQuantity < quantity)
            {
                TempData["Error"] = "Insufficient stock";
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

            TempData["Success"] = $"{perfume.Name} added to cart";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
