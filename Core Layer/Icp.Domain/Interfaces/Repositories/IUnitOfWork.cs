namespace Core.Icp.Domain.Interfaces.Repositories
{
    /// <summary>
    /// رابط Unit of Work برای مدیریت Transaction ها
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repository ها
        ISampleRepository Samples { get; }
        IElementRepository Elements { get; }
        ICRMRepository CRMs { get; }
        IProjectRepository Projects { get; }
        IQualityCheckRepository QualityChecks { get; }  // ← این رو اضافه کن

        // Transaction Operations
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}