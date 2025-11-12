using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Infrastructure.Icp.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Icp.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ICPDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private ISampleRepository? _samples;
        private IElementRepository? _elements;
        private ICRMRepository? _crms;
        private IProjectRepository? _projects;
        private IQualityCheckRepository? _qualityChecks;

        public UnitOfWork(ICPDbContext context)
        {
            _context = context;
        }

        #region Repository Properties

        public ISampleRepository Samples
        {
            get
            {
                _samples ??= new SampleRepository(_context);
                return _samples;
            }
        }

        public IElementRepository Elements
        {
            get
            {
                _elements ??= new ElementRepository(_context);
                return _elements;
            }
        }

        public ICRMRepository CRMs
        {
            get
            {
                _crms ??= new CRMRepository(_context);
                return _crms;
            }
        }

        public IProjectRepository Projects
        {
            get
            {
                _projects ??= new ProjectRepository(_context);
                return _projects;
            }
        }

        public IQualityCheckRepository QualityChecks
        {
            get
            {
                _qualityChecks ??= new QualityCheckRepository(_context);
                return _qualityChecks;
            }
        }

        #endregion

        #region Transaction Operations

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
        }

        #endregion
    }
}