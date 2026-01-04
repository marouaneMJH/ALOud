using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ALOud.DTOs.Products;
using ALOud.DTOs.Categories;
using ALOud.Services;

namespace ALOud.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IProductService productService,
            ICategoryService categoryService,
            IDashboardService dashboardService,
            ILogger<AdminController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _dashboardService = dashboardService;
            _logger = logger;
        }

        // Dashboard with KPIs
        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return View(stats);
        }

        // =====================================================
        // PRODUCTS MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Products(int pageIndex = 1, int pageSize = 10)
        {
            var products = await _productService.GetAllProductsAsync(pageIndex, pageSize);
            return View(products);
        }

        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(new CreateProductDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(dto);
            }

            await _productService.CreateProductAsync(dto);
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            // TODO: We need to get CategoryId from product - update ProductDetailsVM or add to service
            var dto = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = 1 // Temporary - needs fix
            };

            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, UpdateProductDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product update payload {@Dto}", dto);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(dto);
            }

            var success = await _productService.UpdateProductAsync(id, dto);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Products));
        }

        // =====================================================
        // CATEGORIES MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Categories(int pageIndex = 1, int pageSize = 10)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize);
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View(new CreateCategoryDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _categoryService.CreateCategoryAsync(dto);
            return RedirectToAction(nameof(Categories));
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var dto = new UpdateCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, UpdateCategoryDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            var success = await _categoryService.UpdateCategoryAsync(id, dto);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                TempData["Error"] = "Cannot delete category with associated products";
                return RedirectToAction(nameof(Categories));
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
