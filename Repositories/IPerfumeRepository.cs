using ALOud.Models;
using ALOud.DTOs.Perfumes;
using ViewModels;

namespace ALOud.Repositories
{
    /// <summary>
    /// Repository interface for Perfume entity with domain-specific operations
    /// </summary>
    public interface IPerfumeRepository : IRepository<Perfume>
    {
        /// <summary>
        /// Gets paginated perfumes with filtering options
        /// </summary>
        /// <param name="pageIndex">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="searchTerm">Optional search term for name/brand</param>
        /// <param name="brandId">Optional brand filter</param>
        /// <param name="familyId">Optional family filter</param>
        /// <param name="genderProfile">Optional gender profile filter</param>
        /// <returns>Paginated list of perfumes</returns>
        Task<PaginatedList<Perfume>> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null);

        /// <summary>
        /// Gets perfume with all related data (brands, families, notes, etc.)
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The perfume with related data if found, null otherwise</returns>
        Task<Perfume?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Gets perfume with basic brand information
        /// </summary>
        /// <param name="id">The perfume identifier</param>
        /// <returns>The perfume with brand if found, null otherwise</returns>
        Task<Perfume?> GetWithBrandAsync(Guid id);

        /// <summary>
        /// Checks if a perfume exists by name and brand
        /// </summary>
        /// <param name="name">The perfume name</param>
        /// <param name="brandId">The brand identifier</param>
        /// <param name="excludeId">Optional perfume ID to exclude from check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsByNameAndBrandAsync(string name, Guid brandId, Guid? excludeId = null);
    }
}
