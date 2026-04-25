using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Perfumes;
using ViewModels;
using ALOud.Models;
using System.Linq.Expressions;
using ALOud.Repositories;

namespace ALOud.Services.Perfume
{
    /// <summary>
    /// Service layer for perfume business logic and orchestration
    /// </summary>
    public class PerfumeService : IPerfumeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ALOudDbContext _context;

        private static readonly Expression<Func<Models.Perfume, PerfumeDto>> PerfumeDtoProjection = p => new PerfumeDto
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
            Families = p.PerfumeFamilies.Select(pf => pf.Family.Name).ToList(),
            CreatedAt = p.CreatedAt
        };

        /// <summary>
        /// Initializes a new instance of the PerfumeService class
        /// </summary>
        /// <param name="unitOfWork">The unit of work for data access</param>
        /// <param name="context">The database context for complex operations</param>
        public PerfumeService(IUnitOfWork unitOfWork, ALOudDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        /// <summary>
        /// Gets a paginated list of perfumes with optional filtering
        /// </summary>
        /// <param name="pageIndex">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="searchTerm">Optional search term for name/brand</param>
        /// <param name="brandId">Optional brand filter</param>
        /// <param name="familyId">Optional family filter</param>
        /// <param name="genderProfile">Optional gender profile filter</param>
        /// <returns>Paginated list of perfume DTOs</returns>
        public async Task<PaginatedList<PerfumeDto>> GetAllPerfumesAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null)
        {
            var query = _unitOfWork.Perfumes.GetQueryable()
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
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
                .Select(PerfumeDtoProjection)
                .ToListAsync();

            return new PaginatedList<PerfumeDto>(perfumes, totalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a perfume by its identifier
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The perfume DTO if found, null otherwise</returns>
        public async Task<PerfumeDto?> GetPerfumeByIdAsync(Guid id)
        {
            return await _unitOfWork.Perfumes.GetQueryable()
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
                .Where(p => p.Id == id)
                .Select(PerfumeDtoProjection)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets detailed perfume information including all related entities
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The detailed perfume DTO if found, null otherwise</returns>
        public async Task<PerfumeDetailsDto?> GetPerfumeDetailsAsync(Guid id)
        {
            var perfume = await _unitOfWork.Perfumes.GetWithDetailsAsync(id);
            if (perfume == null) return null;
            return MapToPerfumeDetailsDto(perfume);
        }

        /// <summary>
        /// Gets perfume data formatted for editing
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The update perfume DTO if found, null otherwise</returns>
        public async Task<UpdatePerfumeDto?> GetPerfumeForEditAsync(Guid id)
        {
            var perfume = await _unitOfWork.Perfumes.GetWithDetailsAsync(id);
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

        /// <summary>
        /// Creates a new perfume with related entities
        /// </summary>
        /// <param name="dto">The perfume creation data</param>
        /// <returns>The identifier of the created perfume</returns>
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

            await _unitOfWork.Perfumes.AddAsync(perfume);

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

            await _unitOfWork.SaveChangesAsync();
            return perfume.Id;
        }

        /// <summary>
        /// Updates an existing perfume and its related entities
        /// </summary>
        /// <param name="dto">The perfume update data</param>
        /// <returns>True if the perfume was updated, false if not found</returns>
        public async Task<bool> UpdatePerfumeAsync(UpdatePerfumeDto dto)
        {
            var perfume = await _unitOfWork.Perfumes.GetWithDetailsAsync(dto.Id);
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

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a perfume by its identifier
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>True if the perfume was deleted, false if not found</returns>
        public async Task<bool> DeletePerfumeAsync(Guid id)
        {
            var perfume = await _unitOfWork.Perfumes.GetByIdAsync(id);
            if (perfume == null) return false;

            _unitOfWork.Perfumes.Remove(perfume);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Maps a Perfume entity to a PerfumeDetailsDto
        /// </summary>
        /// <param name="perfume">The perfume entity</param>
        /// <returns>The mapped perfume details DTO</returns>
        private static PerfumeDetailsDto MapToPerfumeDetailsDto(Models.Perfume perfume)
        {
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
    }
}
