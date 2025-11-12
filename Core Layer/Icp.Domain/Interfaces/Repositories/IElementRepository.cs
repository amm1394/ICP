using Core.Icp.Domain.Entities.Elements;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    public interface IElementRepository : IRepository<Element>
    {
        Task<IEnumerable<Element>> GetActiveElementsAsync(CancellationToken cancellationToken = default);
        Task<Element?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
        Task<IEnumerable<Element>> GetBySymbolsAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
        Task<Element?> GetWithIsotopesAsync(Guid id, CancellationToken cancellationToken = default);
    }
}