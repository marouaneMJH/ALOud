using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.Services;
using ALOud.DTOs.Perfumes;

namespace Pages.Perfume
{
    public class DetailsModel : PageModel
    {
        private readonly ALOudDbContext _context;
        private readonly ICacheService _cache;

        public PerfumeDetailsDto Perfume { get; set; } = new();

        public DetailsModel(ALOudDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            string key = $"perfume:details:{id}";

            var cached = await _cache.GetAsync<PerfumeDetailsDto>(key);
            if (cached != null)
            {
                Perfume = cached;
                return Page();
            }

            var perfume = await _context.Perfumes
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
                .Include(p => p.PerfumeNotes).ThenInclude(pn => pn.Note)
                .Include(p => p.PerfumeAccords).ThenInclude(pa => pa.Accord)
                .Include(p => p.PerfumeTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.PerfumeSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.PerfumeOccasions).ThenInclude(po => po.Occasion)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfume == null)
                return RedirectToPage("/Index");

            Perfume = new PerfumeDetailsDto
            {
                Id = perfume.Id,
                Name = perfume.Name,
                BrandId = perfume.BrandId,
                BrandName = perfume.Brand.Name,
                Intensity = perfume.Intensity,
                Longevity = perfume.Longevity,
                Sillage = perfume.Sillage,
                GenderProfile = perfume.GenderProfile,
                PriceRange = perfume.PriceRange,
                ImageUrl = perfume.ImageUrl,
                CreatedAt = perfume.CreatedAt,
                Families = perfume.PerfumeFamilies.Select(pf => pf.Family.Name).ToList(),
                Notes = perfume.PerfumeNotes.Select(pn => new PerfumeNoteDto
                {
                    NoteId = pn.NoteId,
                    NoteName = pn.Note.Name,
                    NoteLevel = pn.NoteLevel
                }).ToList(),
                Accords = perfume.PerfumeAccords.Select(pa => new PerfumeAccordDto
                {
                    AccordId = pa.AccordId,
                    AccordName = pa.Accord.Name,
                    Intensity = pa.Intensity
                }).ToList(),
                Tags = perfume.PerfumeTags.Select(pt => pt.Tag.Name).ToList(),
                Seasons = perfume.PerfumeSeasons.Select(ps => ps.Season.Name).ToList(),
                Occasions = perfume.PerfumeOccasions.Select(po => po.Occasion.Name).ToList()
            };

            // Cache for 60 minutes
            await _cache.SetAsync(key, Perfume, TimeSpan.FromMinutes(60));

            return Page();
        }
    }
}
