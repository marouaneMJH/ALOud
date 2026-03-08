using ALOud.Data;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels;

namespace ALOud.Repositories
{
    /// <summary>
    /// Repository implementation for Perfume entity
    /// </summary>
    public class PerfumeRepository : Repository<Perfume>, IPerfumeRepository
    {
        /// <summary>
        /// Initializes a new instance of the PerfumeRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        public PerfumeRepository(ALOudDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets paginated perfumes with filtering options
        /// </summary>
        /// <param name="pageIndex">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="searchTerm">Optional search term for name/brand</param>
        /// <param name="brandId">Optional brand filter</param>
        /// <param name="familyId">Optional family filter</param>
        /// <param name="genderProfile">Optional gender profile filter</param>
        /// <returns>Paginated list of perfumes</returns>
        public async Task<PaginatedList<Perfume>> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null)
        {
            var query = _dbSet
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies)
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
                .ToListAsync();

            return new PaginatedList<Perfume>(perfumes, totalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets perfume with all related data (brands, families, notes, etc.)
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The perfume with related data if found, null otherwise</returns>
        public async Task<Perfume?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
                .Include(p => p.PerfumeNotes).ThenInclude(pn => pn.Note)
                .Include(p => p.PerfumeAccords).ThenInclude(pa => pa.Accord)
                .Include(p => p.PerfumeTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.PerfumeSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.PerfumeOccasions).ThenInclude(po => po.Occasion)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Gets perfume with basic brand information
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The perfume with brand if found, null otherwise</returns>
        public async Task<Perfume?> GetWithBrandAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Checks if a perfume exists by name and brand
        /// </summary>
        /// <param name="name">The perfume name</param>
        /// <param name="brandId">The brand identifier</param>
        /// <param name="excludeId">Optional perfume ID to exclude from check</param>
        /// <returns>True if exists, false otherwise</returns>
        public async Task<bool> ExistsByNameAndBrandAsync(string name, Guid brandId, Guid? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Name == name && p.BrandId == brandId);
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
