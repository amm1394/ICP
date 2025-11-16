using Core.Icp.Domain.Entities.Elements;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Defines the contract for a repository that manages Element entities.
    /// </summary>
    public interface IElementRepository : IRepository<Element>
    {
        Task<IEnumerable<Element>> GetActiveElementsAsync(
            CancellationToken cancellationToken = default);

        Task<Element?> GetBySymbolAsync(
            string symbol,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Element>> GetBySymbolsAsync(
            IEnumerable<string> symbols,
            CancellationToken cancellationToken = default);

        Task<Element?> GetWithIsotopesAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// بیشترین AtomicNumber فعلی در دیتابیس را برمی‌گرداند.
        /// اگر هیچ عنصری وجود نداشته باشد، ۰ برگردانده می‌شود.
        /// </summary>
        Task<int> GetMaxAtomicNumberAsync(
            CancellationToken cancellationToken = default);
    }
}
