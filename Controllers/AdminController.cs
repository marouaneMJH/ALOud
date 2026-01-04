using Microsoft.AspNetCore.Mvc;
using ALOud.Data;
using ALOud.Models;
using ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ALOud.DTOs.Products;
using DTOs.Mappings;


namespace ALOud.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ALOudDbContext _db;
        private readonly ICacheService _cache;
        private readonly ILogger<AdminController> _logger;
        public AdminController(ALOudDbContext db, ICacheService cache, ILogger<AdminController> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products
                .Select(p => new ProductDetailsVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl
                }).ToListAsync();

            return View(products);
        }

        public IActionResult Create()
        {
            return View(new CreateProductDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var product = dto.ToEntity();
            //  new Product
            // {
            //     Name = dto.Name,
            //     Description = dto.Description,
            //     Price = dto.Price,
            //     Stock = dto.Stock,
            //     ImageUrl = dto.ImageUrl,
            //     CategoryId = dto.CategoryId
            // };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
            await _cache.RemoveAsync($"product:details:{product.Id}");

            return RedirectToAction(nameof(Index));
        }


        // Edit product (GET) - use int id to match Product.Id
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            var dto = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product update payload {@Dto}", dto);
                return View(dto);
            }

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Apply(dto);


            await _db.SaveChangesAsync();

            await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
            await _cache.RemoveAsync($"product:details:{id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            _db.Products.Remove(p);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
            await _cache.RemoveAsync($"product:details:{id}");

            return RedirectToAction(nameof(Index));
        }
    }

}
