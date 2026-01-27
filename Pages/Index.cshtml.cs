using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.Models;
using ALOud.Services;
using ALOud.DTOs.Perfumes;
using ViewModels;

namespace Pages
{
    public class IndexModel : PageModel
    {
        private readonly ALOudDbContext _context;
        private readonly ICacheService _cache;

        public IndexModel(ALOudDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public List<PerfumeDto> Perfumes { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();
        public List<Family> Families { get; set; } = new();

        public SearchVM Search { get; set; } = new();

        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 12;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public Guid? SelectedBrandId { get; set; }
        public Guid? SelectedFamilyId { get; set; }
        public string? SelectedGender { get; set; }

        public async Task OnGetAsync(string? query, Guid? brandId, Guid? familyId, string? gender, int pageIndex = 1, int pageSize = 12)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            SelectedBrandId = brandId;
            SelectedFamilyId = familyId;
            SelectedGender = gender;

            Search = new SearchVM
            {
                Query = query
            };

            // Fetch all brands and families for filters
            Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            Families = await _context.Families.OrderBy(f => f.Name).ToListAsync();

            // Build a cache key based on the query parameters
            string key = $"perfumes:list:q={query ?? ""}:brand={brandId?.ToString() ?? ""}:family={familyId?.ToString() ?? ""}:gender={gender ?? ""}:p={pageIndex}:ps={pageSize}";

            var cached = await _cache.GetAsync<(List<PerfumeDto>, int)>(key);
            if (cached.Item1 != null)
            {
                Perfumes = cached.Item1;
                TotalCount = cached.Item2;
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
                return;
            }

            var perfumesQuery = _context.Perfumes
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies)
                    .ThenInclude(pf => pf.Family)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                perfumesQuery = perfumesQuery.Where(p =>
                    p.Name.ToLower().Contains(lowerQuery) ||
                    p.Brand.Name.ToLower().Contains(lowerQuery));
            }

            // Brand filter
            if (brandId.HasValue)
                perfumesQuery = perfumesQuery.Where(p => p.BrandId == brandId.Value);

            // Family filter
            if (familyId.HasValue)
                perfumesQuery = perfumesQuery.Where(p => p.PerfumeFamilies.Any(pf => pf.FamilyId == familyId.Value));

            // Gender filter
            if (!string.IsNullOrWhiteSpace(gender))
                perfumesQuery = perfumesQuery.Where(p => p.GenderProfile == gender);

            perfumesQuery = perfumesQuery.OrderByDescending(p => p.CreatedAt);

            TotalCount = await perfumesQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Perfumes = await perfumesQuery
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new PerfumeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    GenderProfile = p.GenderProfile,
                    PriceRange = p.PriceRange,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Families = p.PerfumeFamilies.Select(pf => pf.Family.Name).ToList(),
                    CreatedAt = p.CreatedAt
                }).ToListAsync();

            // Cache for 5 minutes
            await _cache.SetAsync(key, (Perfumes, TotalCount), TimeSpan.FromMinutes(5));
        }
    }
}
