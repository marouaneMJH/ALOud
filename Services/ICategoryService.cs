using ALOud.DTOs.Categories;

namespace ALOud.Services;

// Category service: handles all category business logic and data operations.
public interface ICategoryService
{
    Task<List<CategoryDetailsDto>> GetAllCategoriesAsync();
    Task<CategoryDetailsDto?> GetCategoryByIdAsync(int id);
    Task<int> CreateCategoryAsync(CreateCategoryDto dto);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<int> GetTotalCategoriesAsync();
}
