using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Entities.Elements;
using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Icp.Data.Repositories
{
    /// <summary>
    /// Repository for Element entities, extending the base repository.
    /// </summary>
    public class ElementRepository : BaseRepository<Element>, IElementRepository
    {
        public ElementRepository(ICPDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Element>> GetActiveElementsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted && e.IsActive)
                .OrderBy(e => e.AtomicNumber)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Element?> GetBySymbolAsync(
            string symbol,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            symbol = symbol.Trim();

            return await _dbSet
                .FirstOrDefaultAsync(
                    e => !e.IsDeleted && e.Symbol == symbol,
                    cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Element>> GetBySymbolsAsync(
            IEnumerable<string> symbols,
            CancellationToken cancellationToken = default)
        {
            if (symbols == null)
                return Array.Empty<Element>();

            var symbolList = symbols
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (symbolList.Count == 0)
                return Array.Empty<Element>();

            return await _dbSet
                .Where(e => !e.IsDeleted && symbolList.Contains(e.Symbol))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Element?> GetWithIsotopesAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Isotopes)
                .FirstOrDefaultAsync(
                    e => !e.IsDeleted && e.Id == id,
                    cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> GetMaxAtomicNumberAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.AtomicNumber)
                .Select(e => e.AtomicNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

    }
}
