using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using ALOud.DTOs.Notes;
using ViewModels;

namespace ALOud.Services.Note
{
    public class NoteService : INoteService
    {
        private readonly ALOudDbContext _context;

        public NoteService(ALOudDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<NoteDto>> GetAllNotesAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, string? category = null)
        {
            var query = _context.Notes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(n => n.Name.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(n => n.Category == category);
            }

            var totalCount = await query.CountAsync();

            var notes = await query
                .OrderBy(n => n.Category).ThenBy(n => n.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Category = n.Category,
                    Description = n.Description,
                    PerfumeCount = n.PerfumeNotes.Count
                })
                .ToListAsync();

            return new PaginatedList<NoteDto>(notes, totalCount, pageIndex, pageSize);
        }

        public async Task<List<NoteSelectDto>> GetAllNotesForSelectAsync()
        {
            return await _context.Notes
                .OrderBy(n => n.Category).ThenBy(n => n.Name)
                .Select(n => new NoteSelectDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Category = n.Category
                })
                .ToListAsync();
        }

        public async Task<List<string>> GetNoteCategoriesAsync()
        {
            return await _context.Notes
                .Where(n => n.Category != null)
                .Select(n => n.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<NoteDto?> GetNoteByIdAsync(Guid id)
        {
            return await _context.Notes
                .Where(n => n.Id == id)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Category = n.Category,
                    Description = n.Description,
                    PerfumeCount = n.PerfumeNotes.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateNoteDto?> GetNoteForEditAsync(Guid id)
        {
            return await _context.Notes
                .Where(n => n.Id == id)
                .Select(n => new UpdateNoteDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Category = n.Category,
                    Description = n.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateNoteAsync(CreateNoteDto dto)
        {
            var note = new Models.Note
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return note.Id;
        }

        public async Task<bool> UpdateNoteAsync(UpdateNoteDto dto)
        {
            var note = await _context.Notes.FindAsync(dto.Id);
            if (note == null) return false;

            note.Name = dto.Name;
            note.Category = dto.Category;
            note.Description = dto.Description;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteNoteAsync(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return false;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> NoteExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Notes.Where(n => n.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(n => n.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
