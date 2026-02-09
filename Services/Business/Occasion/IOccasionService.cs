using ALOud.DTOs.Occasions;
using ViewModels;

namespace ALOud.Services.Occasion
{
    public interface IOccasionService
    {
        Task<PaginatedList<OccasionDto>> GetAllOccasionsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null);
        Task<List<OccasionSelectDto>> GetAllOccasionsForSelectAsync();
        Task<OccasionDto?> GetOccasionByIdAsync(Guid id);
        Task<UpdateOccasionDto?> GetOccasionForEditAsync(Guid id);
        Task<Guid> CreateOccasionAsync(CreateOccasionDto dto);
        Task<bool> UpdateOccasionAsync(UpdateOccasionDto dto);
        Task<bool> DeleteOccasionAsync(Guid id);
        Task<bool> OccasionExistsAsync(string name, Guid? excludeId = null);
    }
}
