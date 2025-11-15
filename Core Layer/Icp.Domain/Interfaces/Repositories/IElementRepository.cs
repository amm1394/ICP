using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Entities.Elements;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Defines the contract for a repository that manages Element entities.
    /// </summary>
    public interface IElementRepository : IRepository<Element>
    {
        /// <summary>
        /// Asynchronously retrieves all elements that are currently active.
        /// </summary>
        Task<IEnumerable<Element>> GetActiveElementsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves an element by its chemical symbol.
        /// </summary>
        Task<Element?> GetBySymbolAsync(
            string symbol,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves a collection of elements based on their chemical symbols.
        /// Used in file import scenarios.
        /// </summary>
        Task<IEnumerable<Element>> GetBySymbolsAsync(
            IEnumerable<string> symbols,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves a specific element by its ID, including its associated isotopes.
        /// </summary>
        Task<Element?> GetWithIsotopesAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the maximum AtomicNumber among all non-deleted elements.
        /// Used to assign unique atomic numbers for auto-created elements.
        /// </summary>
        Task<int> GetMaxAtomicNumberAsync(
            CancellationToken cancellationToken = default);
    }
}
