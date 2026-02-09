using ALOud.DTOs.Perfumes;
using ViewModels;

namespace ALOud.Services.Perfume
{
    public interface IPerfumeService
    {
        Task<PaginatedList<PerfumeDto>> GetAllPerfumesAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null);
        Task<PerfumeDto?> GetPerfumeByIdAsync(Guid id);
        Task<PerfumeDetailsDto?> GetPerfumeDetailsAsync(Guid id);
        Task<UpdatePerfumeDto?> GetPerfumeForEditAsync(Guid id);
        Task<Guid> CreatePerfumeAsync(CreatePerfumeDto dto);
        Task<bool> UpdatePerfumeAsync(UpdatePerfumeDto dto);
        Task<bool> DeletePerfumeAsync(Guid id);
    }
}
