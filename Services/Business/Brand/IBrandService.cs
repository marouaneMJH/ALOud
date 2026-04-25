using ALOud.DTOs.Brands;
using ViewModels;

namespace ALOud.Services.Brand
{
    public interface IBrandService
    {
        Task<PaginatedList<BrandDto>> GetAllBrandsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null);
        Task<List<BrandSelectDto>> GetAllBrandsForSelectAsync();
        Task<BrandDto?> GetBrandByIdAsync(Guid id);
        Task<UpdateBrandDto?> GetBrandForEditAsync(Guid id);
        Task<Guid> CreateBrandAsync(CreateBrandDto dto);
        Task<bool> UpdateBrandAsync(UpdateBrandDto dto);
        Task<bool> DeleteBrandAsync(Guid id);
        Task<bool> BrandExistsAsync(string name, Guid? excludeId = null);
    }
}
