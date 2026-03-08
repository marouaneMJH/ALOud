using System.Linq.Expressions;

namespace ALOud.Repositories
{
    /// <summary>
    /// Generic repository interface providing basic CRUD operations for entities
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its unique identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Finds entities matching the specified predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets a queryable collection of entities
        /// </summary>
        /// <returns>IQueryable of entities</returns>
        IQueryable<T> GetQueryable();

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Adds multiple entities
        /// </summary>
        /// <param name="entities">The entities to add</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        void Update(T entity);

        /// <summary>
        /// Removes an entity
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        void Remove(T entity);

        /// <summary>
        /// Removes multiple entities
        /// </summary>
        /// <param name="entities">The entities to remove</param>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Checks if any entity matches the predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <returns>True if at least one entity matches, false otherwise</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Counts entities matching the predicate
        /// </summary>
        /// <param name="predicate">Optional filter expression</param>
        /// <returns>The count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
