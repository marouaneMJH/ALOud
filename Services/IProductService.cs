using ALOud.DTOs.Products;
using ViewModels;

namespace ALOud.Services;

// Product service: handles all product business logic and data operations.
public interface IProductService
{
    Task<List<ProductDetailsVM>> GetAllProductsAsync();
    Task<PaginatedList<ProductDetailsVM>> GetAllProductsAsync(int pageIndex, int pageSize);
    Task<ProductDetailsVM?> GetProductByIdAsync(int id);
    Task<int> CreateProductAsync(CreateProductDto dto);
    Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
    Task<int> GetTotalProductsAsync();
    Task<int> GetLowStockCountAsync(int threshold = 10);
    Task<int> GetOutOfStockCountAsync();
    Task<decimal> GetTotalInventoryValueAsync();
}
