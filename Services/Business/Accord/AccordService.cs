using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Accords;
using ViewModels;

namespace ALOud.Services.Accord
{
    public class AccordService : IAccordService
    {
        private readonly ALOudDbContext _context;

        public AccordService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<AccordDto>> GetAllAccordsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _context.Accords.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var accords = await query
                .OrderBy(a => a.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AccordDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    PerfumeCount = a.PerfumeAccords.Count
                })
                .ToListAsync();

            return new PaginatedList<AccordDto>(accords, totalCount, pageIndex, pageSize);
        }

        public async Task<List<AccordSelectDto>> GetAllAccordsForSelectAsync()
        {
            return await _context.Accords
                .OrderBy(a => a.Name)
                .Select(a => new AccordSelectDto
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();
        }

        public async Task<AccordDto?> GetAccordByIdAsync(Guid id)
        {
            return await _context.Accords
                .Where(a => a.Id == id)
                .Select(a => new AccordDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    PerfumeCount = a.PerfumeAccords.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateAccordDto?> GetAccordForEditAsync(Guid id)
        {
            return await _context.Accords
                .Where(a => a.Id == id)
                .Select(a => new UpdateAccordDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateAccordAsync(CreateAccordDto dto)
        {
            var accord = new Models.Accord
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Accords.Add(accord);
            await _context.SaveChangesAsync();

            return accord.Id;
        }

        public async Task<bool> UpdateAccordAsync(UpdateAccordDto dto)
        {
            var accord = await _context.Accords.FindAsync(dto.Id);
            if (accord == null) return false;

            accord.Name = dto.Name;
            accord.Description = dto.Description;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAccordAsync(Guid id)
        {
            var accord = await _context.Accords.FindAsync(id);
            if (accord == null) return false;

            _context.Accords.Remove(accord);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AccordExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Accords.Where(a => a.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
