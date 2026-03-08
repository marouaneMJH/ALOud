using ALOud.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ALOud.Repositories
{
    /// <summary>
    /// Unit of Work implementation coordinating repository operations and transaction management
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ALOudDbContext _context;
        private IDbContextTransaction? _transaction;
        private IPerfumeRepository? _perfumeRepository;

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class
        /// </summary>
        /// <param name="context">The database context</param>
        public UnitOfWork(ALOudDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the perfume repository
        /// </summary>
        public IPerfumeRepository Perfumes => _perfumeRepository ??= new PerfumeRepository(_context);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Disposes the unit of work and releases resources
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
