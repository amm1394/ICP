using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Enums;
using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Icp.Data.Repositories
{
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ICPDbContext context) : base(context)
        {
        }

        public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _dbSet
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Include(p => p.Samples)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<IEnumerable<Project>> GetRecentProjectsAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            if (count <= 0) count = 10;

            return await _dbSet
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetByStatusAsync(
            ProjectStatus status,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetWithSamplesAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Samples)
                .FirstOrDefaultAsync(
                    p => !p.IsDeleted && p.Id == id,
                    cancellationToken);
        }

        public async Task<Project?> GetByIdWithSamplesAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // می‌تونی همین متد را دوباره استفاده کنی
            return await GetWithSamplesAsync(id, cancellationToken);
        }

        public async Task<Project?> GetWithFullDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Samples)
                    .ThenInclude(s => s.Measurements)
                .FirstOrDefaultAsync(
                    p => !p.IsDeleted && p.Id == id,
                    cancellationToken);
        }
    }
}
