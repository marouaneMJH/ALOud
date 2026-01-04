using ALOud.Data;
using ALOud.DTOs.Categories;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

// Category service: handles category CRUD operations and statistics.
public class CategoryService : ICategoryService
{
    private readonly ALOudDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ALOudDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CategoryDetailsDto>> GetAllCategoriesAsync()
    {
        return await _db.Categories
            .Select(c => new CategoryDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = c.Products.Count
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CategoryDetailsDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();

        return category;
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);
        return category.Id;
    }

    public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return false;

        category.Name = dto.Name;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Category updated: {CategoryId} - {CategoryName}", category.Id, category.Name);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return false;

        // Check if category has products
        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            _logger.LogWarning("Cannot delete category {CategoryId} - has associated products", id);
            return false;
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Category deleted: {CategoryId}", id);
        return true;
    }

    public async Task<int> GetTotalCategoriesAsync()
    {
        return await _db.Categories.CountAsync();
    }
}
