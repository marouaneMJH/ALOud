using System.Diagnostics;
using ALOud.Data;
using ALOud.DTOs.Products;
using ALOud.Models;
using DTOs.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels;

namespace ALOud.Services;

// Product service: handles product CRUD and business logic with caching support.
public class ProductService : IProductService
{
    private readonly ALOudDbContext _db;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ALOudDbContext db, ICacheService cache, ILogger<ProductService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ProductDetailsVM>> GetAllProductsAsync()
    {
        return await _db.Products
            .Select(p => new ProductDetailsVM
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            }).ToListAsync();
    }

    public async Task<PaginatedList<ProductDetailsVM>> GetAllProductsAsync(int pageIndex, int pageSize)
    {
        var totalCount = await _db.Products.CountAsync();

        var products = await _db.Products
            .OrderByDescending(p => p.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDetailsVM
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            }).ToListAsync();

        return new PaginatedList<ProductDetailsVM>(products, totalCount, pageIndex, pageSize);
    }

    public async Task<List<object>> SearchProductsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<object>();

        var lowerQuery = query.ToLower();

        return await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Name.ToLower().Contains(lowerQuery) ||
                       p.Description.ToLower().Contains(lowerQuery))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.ImageUrl,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
            })
            .Take(20)
            .Cast<object>()
            .ToListAsync();
    }

    public async Task<ProductDetailsVM?> GetProductByIdAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return null;

        return new ProductDetailsVM
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl

        };
    }
    public async Task<UpdateProductDto?> GetUpdateProductDtoAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return null;

        return new UpdateProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
        };
    }

    public async Task<int> CreateProductAsync(CreateProductDto dto)
    {
        var product = dto.ToEntity();
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
        await _cache.RemoveAsync($"product:details:{product.Id}");

        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
        return product.Id;
    }

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return false;

        product.Apply(dto);
        await _db.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
        await _cache.RemoveAsync($"product:details:{id}");

        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", product.Id, product.Name);
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync("products:list:q=:min=:max=:s=");
        await _cache.RemoveAsync($"product:details:{id}");

        _logger.LogInformation("Product deleted: {ProductId}", id);
        return true;
    }

    public async Task<int> GetTotalProductsAsync()
    {
        return await _db.Products.CountAsync();
    }

    public async Task<int> GetLowStockCountAsync(int threshold = 10)
    {
        return await _db.Products.CountAsync(p => p.Stock > 0 && p.Stock <= threshold);
    }

    public async Task<int> GetOutOfStockCountAsync()
    {
        return await _db.Products.CountAsync(p => p.Stock == 0);
    }

    public async Task<decimal> GetTotalInventoryValueAsync()
    {
        return await _db.Products.SumAsync(p => p.Price * p.Stock);
    }
}
