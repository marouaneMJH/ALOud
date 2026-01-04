using ALOud.DTOs.Categories;
using ViewModels;

namespace ALOud.Services;

// Category service: handles all category business logic and data operations.
public interface ICategoryService
{
    Task<List<CategoryDetailsDto>> GetAllCategoriesAsync();
    Task<PaginatedList<CategoryDetailsDto>> GetAllCategoriesAsync(int pageIndex, int pageSize);
    Task<CategoryDetailsDto?> GetCategoryByIdAsync(int id);
    Task<int> CreateCategoryAsync(CreateCategoryDto dto);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<int> GetTotalCategoriesAsync();
}
