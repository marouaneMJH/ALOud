namespace ALOud.Repositories
{
    /// <summary>
    /// Unit of Work pattern interface for coordinating repository operations and transaction management
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the perfume repository
        /// </summary>
        IPerfumeRepository Perfumes { get; }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
