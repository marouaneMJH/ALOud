using ALOud.DTOs.Notes;
using ViewModels;

namespace ALOud.Services.Note
{
    public interface INoteService
    {
        Task<PaginatedList<NoteDto>> GetAllNotesAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, string? category = null);
        Task<List<NoteSelectDto>> GetAllNotesForSelectAsync();
        Task<List<string>> GetNoteCategoriesAsync();
        Task<NoteDto?> GetNoteByIdAsync(Guid id);
        Task<UpdateNoteDto?> GetNoteForEditAsync(Guid id);
        Task<Guid> CreateNoteAsync(CreateNoteDto dto);
        Task<bool> UpdateNoteAsync(UpdateNoteDto dto);
        Task<bool> DeleteNoteAsync(Guid id);
        Task<bool> NoteExistsAsync(string name, Guid? excludeId = null);
    }
}
