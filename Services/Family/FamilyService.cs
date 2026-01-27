using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Families;
using ViewModels;

namespace ALOud.Services.Family
{
    public class FamilyService : IFamilyService
    {
        private readonly ALOudDbContext _context;

        public FamilyService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<FamilyDto>> GetAllFamiliesAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _context.Families.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(f => f.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var families = await query
                .OrderBy(f => f.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FamilyDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    PerfumeCount = f.PerfumeFamilies.Count
                })
                .ToListAsync();

            return new PaginatedList<FamilyDto>(families, totalCount, pageIndex, pageSize);
        }

        public async Task<List<FamilySelectDto>> GetAllFamiliesForSelectAsync()
        {
            return await _context.Families
                .OrderBy(f => f.Name)
                .Select(f => new FamilySelectDto
                {
                    Id = f.Id,
                    Name = f.Name
                })
                .ToListAsync();
        }

        public async Task<FamilyDto?> GetFamilyByIdAsync(Guid id)
        {
            return await _context.Families
                .Where(f => f.Id == id)
                .Select(f => new FamilyDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    PerfumeCount = f.PerfumeFamilies.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateFamilyDto?> GetFamilyForEditAsync(Guid id)
        {
            return await _context.Families
                .Where(f => f.Id == id)
                .Select(f => new UpdateFamilyDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateFamilyAsync(CreateFamilyDto dto)
        {
            var family = new Models.Family
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            return family.Id;
        }

        public async Task<bool> UpdateFamilyAsync(UpdateFamilyDto dto)
        {
            var family = await _context.Families.FindAsync(dto.Id);
            if (family == null) return false;

            family.Name = dto.Name;
            family.Description = dto.Description;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteFamilyAsync(Guid id)
        {
            var family = await _context.Families.FindAsync(id);
            if (family == null) return false;

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> FamilyExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Families.Where(f => f.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(f => f.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
