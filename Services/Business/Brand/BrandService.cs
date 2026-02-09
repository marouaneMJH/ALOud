using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Brands;
using ViewModels;

namespace ALOud.Services.Brand
{
    public class BrandService : IBrandService
    {
        private readonly ALOudDbContext _context;

        public BrandService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<BrandDto>> GetAllBrandsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _context.Brands.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var brands = await query
                .OrderBy(b => b.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    PerfumeCount = b.Perfumes.Count
                })
                .ToListAsync();

            return new PaginatedList<BrandDto>(brands, totalCount, pageIndex, pageSize);
        }

        public async Task<List<BrandSelectDto>> GetAllBrandsForSelectAsync()
        {
            return await _context.Brands
                .OrderBy(b => b.Name)
                .Select(b => new BrandSelectDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();
        }

        public async Task<BrandDto?> GetBrandByIdAsync(Guid id)
        {
            return await _context.Brands
                .Where(b => b.Id == id)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    PerfumeCount = b.Perfumes.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateBrandDto?> GetBrandForEditAsync(Guid id)
        {
            return await _context.Brands
                .Where(b => b.Id == id)
                .Select(b => new UpdateBrandDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateBrandAsync(CreateBrandDto dto)
        {
            var brand = new Models.Brand
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return brand.Id;
        }

        public async Task<bool> UpdateBrandAsync(UpdateBrandDto dto)
        {
            var brand = await _context.Brands.FindAsync(dto.Id);
            if (brand == null) return false;

            brand.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBrandAsync(Guid id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BrandExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Brands.Where(b => b.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(b => b.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
