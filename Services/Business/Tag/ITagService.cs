using ALOud.DTOs.Tags;
using ViewModels;

namespace ALOud.Services.Tag
{
    public interface ITagService
    {
        Task<PaginatedList<TagDto>> GetAllTagsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null);
        Task<List<TagSelectDto>> GetAllTagsForSelectAsync();
        Task<TagDto?> GetTagByIdAsync(Guid id);
        Task<UpdateTagDto?> GetTagForEditAsync(Guid id);
        Task<Guid> CreateTagAsync(CreateTagDto dto);
        Task<bool> UpdateTagAsync(UpdateTagDto dto);
        Task<bool> DeleteTagAsync(Guid id);
        Task<bool> TagExistsAsync(string name, Guid? excludeId = null);
    }
}
