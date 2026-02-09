using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Occasions;
using ViewModels;

namespace ALOud.Services.Occasion
{
    public class OccasionService : IOccasionService
    {
        private readonly ALOudDbContext _context;

        public OccasionService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<OccasionDto>> GetAllOccasionsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _context.Occasions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o => o.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var occasions = await query
                .OrderBy(o => o.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OccasionDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    PerfumeCount = o.PerfumeOccasions.Count
                })
                .ToListAsync();

            return new PaginatedList<OccasionDto>(occasions, totalCount, pageIndex, pageSize);
        }

        public async Task<List<OccasionSelectDto>> GetAllOccasionsForSelectAsync()
        {
            return await _context.Occasions
                .OrderBy(o => o.Name)
                .Select(o => new OccasionSelectDto
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .ToListAsync();
        }

        public async Task<OccasionDto?> GetOccasionByIdAsync(Guid id)
        {
            return await _context.Occasions
                .Where(o => o.Id == id)
                .Select(o => new OccasionDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    PerfumeCount = o.PerfumeOccasions.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateOccasionDto?> GetOccasionForEditAsync(Guid id)
        {
            return await _context.Occasions
                .Where(o => o.Id == id)
                .Select(o => new UpdateOccasionDto
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateOccasionAsync(CreateOccasionDto dto)
        {
            var occasion = new Models.Occasion
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.Occasions.Add(occasion);
            await _context.SaveChangesAsync();

            return occasion.Id;
        }

        public async Task<bool> UpdateOccasionAsync(UpdateOccasionDto dto)
        {
            var occasion = await _context.Occasions.FindAsync(dto.Id);
            if (occasion == null) return false;

            occasion.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteOccasionAsync(Guid id)
        {
            var occasion = await _context.Occasions.FindAsync(id);
            if (occasion == null) return false;

            _context.Occasions.Remove(occasion);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> OccasionExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Occasions.Where(o => o.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(o => o.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
