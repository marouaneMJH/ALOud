using ALOud.DTOs.Families;
using ViewModels;

namespace ALOud.Services.Family
{
    public interface IFamilyService
    {
        Task<PaginatedList<FamilyDto>> GetAllFamiliesAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null);
        Task<List<FamilySelectDto>> GetAllFamiliesForSelectAsync();
        Task<FamilyDto?> GetFamilyByIdAsync(Guid id);
        Task<UpdateFamilyDto?> GetFamilyForEditAsync(Guid id);
        Task<Guid> CreateFamilyAsync(CreateFamilyDto dto);
        Task<bool> UpdateFamilyAsync(UpdateFamilyDto dto);
        Task<bool> DeleteFamilyAsync(Guid id);
        Task<bool> FamilyExistsAsync(string name, Guid? excludeId = null);
    }
}
