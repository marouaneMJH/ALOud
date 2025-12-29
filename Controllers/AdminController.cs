using Microsoft.AspNetCore.Mvc;
using ALOud.Data;
using ALOud.Models;
using ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ALOud.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ALOudDbContext _db;
        private readonly ICacheService _cache;

        public AdminController(ALOudDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products
                .Select(p => new ProductDetailsVM
                {
                    Id = p.Id.ToString(),
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
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            // invalidate simple list cache and product details cache
            await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
            await _cache.RemoveAsync($"product:details:{model.Id}");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            object key;
            if (Guid.TryParse(id, out var g)) key = g;
            else if (int.TryParse(id, out var i)) key = i;
            else return BadRequest();

            var p = await _db.Products.FindAsync(new object[] { key });
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product model)
        {
            // validate route id matches model id
            if (!string.IsNullOrEmpty(id))
            {
                // compare string forms
                if (model.Id.ToString() != id) return BadRequest();
            }
            if (!ModelState.IsValid) return View(model);

            _db.Products.Update(model);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
            await _cache.RemoveAsync($"product:details:{model.Id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            object key;
            if (Guid.TryParse(id, out var g)) key = g;
            else if (int.TryParse(id, out var i)) key = i;
            else return BadRequest();

            var p = await _db.Products.FindAsync(new object[] { key });
            if (p == null) return NotFound();

            _db.Products.Remove(p);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
            await _cache.RemoveAsync($"product:details:{id}");

            return RedirectToAction(nameof(Index));
        }
    }
}
