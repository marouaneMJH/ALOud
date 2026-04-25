using ALOud.DTOs.Seasons;
using ViewModels;

namespace ALOud.Services.Season
{
    public interface ISeasonService
    {
        Task<PaginatedList<SeasonDto>> GetAllSeasonsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null);
        Task<List<SeasonSelectDto>> GetAllSeasonsForSelectAsync();
        Task<SeasonDto?> GetSeasonByIdAsync(Guid id);
        Task<UpdateSeasonDto?> GetSeasonForEditAsync(Guid id);
        Task<Guid> CreateSeasonAsync(CreateSeasonDto dto);
        Task<bool> UpdateSeasonAsync(UpdateSeasonDto dto);
        Task<bool> DeleteSeasonAsync(Guid id);
        Task<bool> SeasonExistsAsync(string name, Guid? excludeId = null);
    }
}
