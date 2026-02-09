

using ALOud.Data;
using Microsoft.EntityFrameworkCore;


/**
 *  Extract all perfume-related data from SQL and return a clean,
 *  aggregated domain object per perfume.
*/
public class ProductDataExtractor : IProductDataExtractor
{
    private readonly ALOudDbContext _db;

    public ProductDataExtractor(ALOudDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PerfumeRagSource>> ExtractAllAsync(
        CancellationToken cancellationToken = default)
    {
        var perfumes = await _db.Perfumes
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
            .Include(p => p.PerfumeNotes).ThenInclude(pn => pn.Note)
            .Include(p => p.PerfumeAccords).ThenInclude(pa => pa.Accord)
            .Include(p => p.PerfumeTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.PerfumeSeasons).ThenInclude(ps => ps.Season)
            .Include(p => p.PerfumeOccasions).ThenInclude(po => po.Occasion)
            .ToListAsync(cancellationToken);

        return perfumes.Select(p => new PerfumeRagSource
        {
            Id = p.Id,
            Name = p.Name,
            Brand = p.Brand.Name,

            Intensity = p.Intensity,
            Longevity = p.Longevity,
            Sillage = p.Sillage,
            GenderProfile = p.GenderProfile,
            PriceRange = p.PriceRange,

            Families = p.PerfumeFamilies
                .Select(f => f.Family.Name)
                .ToList(),

            Notes = p.PerfumeNotes
                .Select(n => new PerfumeNoteInfo
                {
                    Name = n.Note.Name,
                    Category = n.Note.Category,
                    Level = n.NoteLevel
                })
                .ToList(),

            Accords = p.PerfumeAccords
                .Select(a => new PerfumeAccordInfo
                {
                    Name = a.Accord.Name,
                    Intensity = a.Intensity
                })
                .ToList(),

            Tags = p.PerfumeTags
                .Select(t => t.Tag.Name)
                .ToList(),

            Seasons = p.PerfumeSeasons
                .Select(s => s.Season.Name)
                .ToList(),

            Occasions = p.PerfumeOccasions
                .Select(o => o.Occasion.Name)
                .ToList()
        }).ToList();
    }
}
