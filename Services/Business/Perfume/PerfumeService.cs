using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Perfumes;
using ViewModels;
using ALOud.Models;

namespace ALOud.Services.Perfume
{
    public class PerfumeService : IPerfumeService
    {
        private readonly ALOudDbContext _context;

        public PerfumeService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<PerfumeDto>> GetAllPerfumesAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null)
        {
            var query = _context.Perfumes
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Brand.Name.Contains(searchTerm));
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            if (familyId.HasValue)
            {
                query = query.Where(p => p.PerfumeFamilies.Any(pf => pf.FamilyId == familyId.Value));
            }

            if (!string.IsNullOrWhiteSpace(genderProfile))
            {
                query = query.Where(p => p.GenderProfile == genderProfile);
            }

            var totalCount = await query.CountAsync();

            var perfumes = await query
                .OrderBy(p => p.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PerfumeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Intensity = p.Intensity,
                    Longevity = p.Longevity,
                    Sillage = p.Sillage,
                    GenderProfile = p.GenderProfile,
                    PriceRange = p.PriceRange,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Families = p.PerfumeFamilies.Select(pf => pf.Family.Name).ToList(),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return new PaginatedList<PerfumeDto>(perfumes, totalCount, pageIndex, pageSize);
        }

        public async Task<PerfumeDto?> GetPerfumeByIdAsync(Guid id)
        {
            return await _context.Perfumes
                .Include(p => p.Brand)
                .Where(p => p.Id == id)
                .Select(p => new PerfumeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Intensity = p.Intensity,
                    Longevity = p.Longevity,
                    Sillage = p.Sillage,
                    GenderProfile = p.GenderProfile,
                    PriceRange = p.PriceRange,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PerfumeDetailsDto?> GetPerfumeDetailsAsync(Guid id)
        {
            var perfume = await _context.Perfumes
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
                .Include(p => p.PerfumeNotes).ThenInclude(pn => pn.Note)
                .Include(p => p.PerfumeAccords).ThenInclude(pa => pa.Accord)
                .Include(p => p.PerfumeTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.PerfumeSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.PerfumeOccasions).ThenInclude(po => po.Occasion)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfume == null) return null;

            return new PerfumeDetailsDto
            {
                Id = perfume.Id,
                Name = perfume.Name,
                Intensity = perfume.Intensity,
                Longevity = perfume.Longevity,
                Sillage = perfume.Sillage,
                GenderProfile = perfume.GenderProfile,
                PriceRange = perfume.PriceRange,
                Price = perfume.Price,
                StockQuantity = perfume.StockQuantity,
                Description = perfume.Description,
                ImageUrl = perfume.ImageUrl,
                CreatedAt = perfume.CreatedAt,
                BrandId = perfume.BrandId,
                BrandName = perfume.Brand.Name,
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
        }

        public async Task<UpdatePerfumeDto?> GetPerfumeForEditAsync(Guid id)
        {
            var perfume = await _context.Perfumes
                .Include(p => p.PerfumeFamilies)
                .Include(p => p.PerfumeNotes)
                .Include(p => p.PerfumeAccords)
                .Include(p => p.PerfumeTags)
                .Include(p => p.PerfumeSeasons)
                .Include(p => p.PerfumeOccasions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfume == null) return null;

            return new UpdatePerfumeDto
            {
                Id = perfume.Id,
                Name = perfume.Name,
                Intensity = perfume.Intensity,
                Longevity = perfume.Longevity,
                Sillage = perfume.Sillage,
                GenderProfile = perfume.GenderProfile,
                PriceRange = perfume.PriceRange,
                Price = perfume.Price,
                Description = perfume.Description,
                ImageUrl = perfume.ImageUrl,
                BrandId = perfume.BrandId,
                FamilyIds = perfume.PerfumeFamilies.Select(pf => pf.FamilyId).ToList(),
                NoteSelections = perfume.PerfumeNotes.Select(pn => new PerfumeNoteSelectionDto
                {
                    NoteId = pn.NoteId,
                    NoteLevel = pn.NoteLevel
                }).ToList(),
                AccordSelections = perfume.PerfumeAccords.Select(pa => new PerfumeAccordSelectionDto
                {
                    AccordId = pa.AccordId,
                    Intensity = pa.Intensity
                }).ToList(),
                TagIds = perfume.PerfumeTags.Select(pt => pt.TagId).ToList(),
                SeasonIds = perfume.PerfumeSeasons.Select(ps => ps.SeasonId).ToList(),
                OccasionIds = perfume.PerfumeOccasions.Select(po => po.OccasionId).ToList()
            };
        }

        public async Task<Guid> CreatePerfumeAsync(CreatePerfumeDto dto)
        {
            var perfume = new Models.Perfume
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Intensity = dto.Intensity,
                Longevity = dto.Longevity,
                Sillage = dto.Sillage,
                GenderProfile = dto.GenderProfile,
                PriceRange = dto.PriceRange,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                BrandId = dto.BrandId
            };

            _context.Perfumes.Add(perfume);

            // Add families
            foreach (var familyId in dto.FamilyIds)
            {
                _context.PerfumeFamilies.Add(new PerfumeFamily { PerfumeId = perfume.Id, FamilyId = familyId });
            }

            // Add notes
            foreach (var note in dto.NoteSelections)
            {
                _context.PerfumeNotes.Add(new PerfumeNote { PerfumeId = perfume.Id, NoteId = note.NoteId, NoteLevel = note.NoteLevel });
            }

            // Add accords
            foreach (var accord in dto.AccordSelections)
            {
                _context.PerfumeAccords.Add(new PerfumeAccord { PerfumeId = perfume.Id, AccordId = accord.AccordId, Intensity = accord.Intensity });
            }

            // Add tags
            foreach (var tagId in dto.TagIds)
            {
                _context.PerfumeTags.Add(new PerfumeTag { PerfumeId = perfume.Id, TagId = tagId });
            }

            // Add seasons
            foreach (var seasonId in dto.SeasonIds)
            {
                _context.PerfumeSeasons.Add(new PerfumeSeason { PerfumeId = perfume.Id, SeasonId = seasonId });
            }

            // Add occasions
            foreach (var occasionId in dto.OccasionIds)
            {
                _context.PerfumeOccasions.Add(new PerfumeOccasion { PerfumeId = perfume.Id, OccasionId = occasionId });
            }

            await _context.SaveChangesAsync();
            return perfume.Id;
        }

        public async Task<bool> UpdatePerfumeAsync(UpdatePerfumeDto dto)
        {
            var perfume = await _context.Perfumes
                .Include(p => p.PerfumeFamilies)
                .Include(p => p.PerfumeNotes)
                .Include(p => p.PerfumeAccords)
                .Include(p => p.PerfumeTags)
                .Include(p => p.PerfumeSeasons)
                .Include(p => p.PerfumeOccasions)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (perfume == null) return false;

            // Update basic properties
            perfume.Name = dto.Name;
            perfume.Intensity = dto.Intensity;
            perfume.Longevity = dto.Longevity;
            perfume.Sillage = dto.Sillage;
            perfume.GenderProfile = dto.GenderProfile;
            perfume.PriceRange = dto.PriceRange;
            perfume.Price = dto.Price;
            perfume.StockQuantity = dto.StockQuantity;
            perfume.Description = dto.Description;
            perfume.ImageUrl = dto.ImageUrl;
            perfume.BrandId = dto.BrandId;

            // Update families
            _context.PerfumeFamilies.RemoveRange(perfume.PerfumeFamilies);
            foreach (var familyId in dto.FamilyIds)
            {
                _context.PerfumeFamilies.Add(new PerfumeFamily { PerfumeId = perfume.Id, FamilyId = familyId });
            }

            // Update notes
            _context.PerfumeNotes.RemoveRange(perfume.PerfumeNotes);
            foreach (var note in dto.NoteSelections)
            {
                _context.PerfumeNotes.Add(new PerfumeNote { PerfumeId = perfume.Id, NoteId = note.NoteId, NoteLevel = note.NoteLevel });
            }

            // Update accords
            _context.PerfumeAccords.RemoveRange(perfume.PerfumeAccords);
            foreach (var accord in dto.AccordSelections)
            {
                _context.PerfumeAccords.Add(new PerfumeAccord { PerfumeId = perfume.Id, AccordId = accord.AccordId, Intensity = accord.Intensity });
            }

            // Update tags
            _context.PerfumeTags.RemoveRange(perfume.PerfumeTags);
            foreach (var tagId in dto.TagIds)
            {
                _context.PerfumeTags.Add(new PerfumeTag { PerfumeId = perfume.Id, TagId = tagId });
            }

            // Update seasons
            _context.PerfumeSeasons.RemoveRange(perfume.PerfumeSeasons);
            foreach (var seasonId in dto.SeasonIds)
            {
                _context.PerfumeSeasons.Add(new PerfumeSeason { PerfumeId = perfume.Id, SeasonId = seasonId });
            }

            // Update occasions
            _context.PerfumeOccasions.RemoveRange(perfume.PerfumeOccasions);
            foreach (var occasionId in dto.OccasionIds)
            {
                _context.PerfumeOccasions.Add(new PerfumeOccasion { PerfumeId = perfume.Id, OccasionId = occasionId });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePerfumeAsync(Guid id)
        {
            var perfume = await _context.Perfumes.FindAsync(id);
            if (perfume == null) return false;

            _context.Perfumes.Remove(perfume);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
