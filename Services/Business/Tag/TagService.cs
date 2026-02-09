using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Tags;
using ViewModels;

namespace ALOud.Services.Tag
{
    public class TagService : ITagService
    {
        private readonly ALOudDbContext _context;

        public TagService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<TagDto>> GetAllTagsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _context.Tags.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var tags = await query
                .OrderBy(t => t.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    PerfumeCount = t.PerfumeTags.Count
                })
                .ToListAsync();

            return new PaginatedList<TagDto>(tags, totalCount, pageIndex, pageSize);
        }

        public async Task<List<TagSelectDto>> GetAllTagsForSelectAsync()
        {
            return await _context.Tags
                .OrderBy(t => t.Name)
                .Select(t => new TagSelectDto
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();
        }

        public async Task<TagDto?> GetTagByIdAsync(Guid id)
        {
            return await _context.Tags
                .Where(t => t.Id == id)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    PerfumeCount = t.PerfumeTags.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateTagDto?> GetTagForEditAsync(Guid id)
        {
            return await _context.Tags
                .Where(t => t.Id == id)
                .Select(t => new UpdateTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateTagAsync(CreateTagDto dto)
        {
            var tag = new Models.Tag
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return tag.Id;
        }

        public async Task<bool> UpdateTagAsync(UpdateTagDto dto)
        {
            var tag = await _context.Tags.FindAsync(dto.Id);
            if (tag == null) return false;

            tag.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTagAsync(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return false;

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TagExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Tags.Where(t => t.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
