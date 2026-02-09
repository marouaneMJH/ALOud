using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Seasons;
using ViewModels;

namespace ALOud.Services.Season
{
    public class SeasonService : ISeasonService
    {
        private readonly ALOudDbContext _context;

        public SeasonService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<SeasonDto>> GetAllSeasonsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _context.Seasons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var seasons = await query
                .OrderBy(s => s.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SeasonDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PerfumeCount = s.PerfumeSeasons.Count
                })
                .ToListAsync();

            return new PaginatedList<SeasonDto>(seasons, totalCount, pageIndex, pageSize);
        }

        public async Task<List<SeasonSelectDto>> GetAllSeasonsForSelectAsync()
        {
            return await _context.Seasons
                .OrderBy(s => s.Name)
                .Select(s => new SeasonSelectDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();
        }

        public async Task<SeasonDto?> GetSeasonByIdAsync(Guid id)
        {
            return await _context.Seasons
                .Where(s => s.Id == id)
                .Select(s => new SeasonDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PerfumeCount = s.PerfumeSeasons.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateSeasonDto?> GetSeasonForEditAsync(Guid id)
        {
            return await _context.Seasons
                .Where(s => s.Id == id)
                .Select(s => new UpdateSeasonDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateSeasonAsync(CreateSeasonDto dto)
        {
            var season = new Models.Season
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

            return season.Id;
        }

        public async Task<bool> UpdateSeasonAsync(UpdateSeasonDto dto)
        {
            var season = await _context.Seasons.FindAsync(dto.Id);
            if (season == null) return false;

            season.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSeasonAsync(Guid id)
        {
            var season = await _context.Seasons.FindAsync(id);
            if (season == null) return false;

            _context.Seasons.Remove(season);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SeasonExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Seasons.Where(s => s.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
