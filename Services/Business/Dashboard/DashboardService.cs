using ALOud.Data;
using ALOud.DTOs.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

// Dashboard service: aggregates KPIs and statistics from perfume catalog.
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
            TotalPerfumes = await _db.Perfumes.CountAsync(),
            TotalBrands = await _db.Brands.CountAsync(),
            TotalFamilies = await _db.Families.CountAsync(),
            TotalNotes = await _db.Notes.CountAsync(),
            TotalAccords = await _db.Accords.CountAsync(),
            TotalTags = await _db.Tags.CountAsync(),
            TotalSeasons = await _db.Seasons.CountAsync(),
            TotalOccasions = await _db.Occasions.CountAsync()
        };

        // Top brands by perfume count
        stats.TopBrands = await _db.Brands
            .Select(b => new BrandStatsDto
            {
                BrandId = b.Id,
                BrandName = b.Name,
                PerfumeCount = b.Perfumes.Count
            })
            .OrderByDescending(b => b.PerfumeCount)
            .Take(5)
            .ToListAsync();

        // Top families by perfume count
        stats.TopFamilies = await _db.Families
            .Select(f => new FamilyStatsDto
            {
                FamilyId = f.Id,
                FamilyName = f.Name,
                PerfumeCount = f.PerfumeFamilies.Count
            })
            .OrderByDescending(f => f.PerfumeCount)
            .Take(5)
            .ToListAsync();

        // Recent perfumes
        stats.RecentPerfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .OrderByDescending(p => p.Id)
            .Take(5)
            .Select(p => new RecentPerfumeDto
            {
                PerfumeId = p.Id,
                PerfumeName = p.Name,
                BrandName = p.Brand.Name,
                GenderProfile = p.GenderProfile,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        _logger.LogInformation("Dashboard stats generated: {TotalPerfumes} perfumes, {TotalBrands} brands",
            stats.TotalPerfumes, stats.TotalBrands);

        return stats;
    }
}
