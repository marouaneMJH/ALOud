using ALOud.Data;
using ALOud.DTOs.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

// Dashboard service: aggregates KPIs and statistics from products and categories.
public class DashboardService : IDashboardService
{
    private readonly ALOudDbContext _db;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ALOudDbContext db, ILogger<DashboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var stats = new DashboardStatsDto
        {
            TotalProducts = await _db.Products.CountAsync(),
            TotalCategories = await _db.Categories.CountAsync(),
            LowStockProducts = await _db.Products.CountAsync(p => p.Stock > 0 && p.Stock <= 10),
            OutOfStockProducts = await _db.Products.CountAsync(p => p.Stock == 0),
            TotalInventoryValue = await _db.Products.SumAsync(p => p.Price * p.Stock)
        };

        // Category statistics
        stats.CategoryStats = await _db.Categories
            .Select(c => new CategoryStatsDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ProductCount = c.Products.Count,
                TotalStock = c.Products.Sum(p => p.Stock)
            })
            .OrderByDescending(c => c.ProductCount)
            .ToListAsync();

        // Low stock alerts
        stats.LowStockAlerts = await _db.Products
            .Where(p => p.Stock <= 10)
            .OrderBy(p => p.Stock)
            .Take(10)
            .Select(p => new ProductStockAlertDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        _logger.LogInformation("Dashboard stats generated: {TotalProducts} products, {TotalCategories} categories",
            stats.TotalProducts, stats.TotalCategories);

        return stats;
    }
}
