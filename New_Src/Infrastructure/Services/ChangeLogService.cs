using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ChangeLogService : IChangeLogService
{
    private readonly IsatisDbContext _db;
    private readonly ILogger<ChangeLogService> _logger;

    public ChangeLogService(IsatisDbContext db, ILogger<ChangeLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogChangeAsync(Guid projectId, string changeType, string solutionLabel,
        string? element, object? oldValue, object? newValue, string? details = null, string? userId = null)
    {
        try
        {
            var log = new ChangeLog
            {
                ProjectId = projectId,
                ChangeType = changeType,
                SolutionLabel = solutionLabel,
                Element = element,
                OldValue = oldValue?.ToString(),
                NewValue = newValue?.ToString(),
                Details = details,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _db.ChangeLogs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogDebug("Logged change: {ChangeType} for {SolutionLabel} in project {ProjectId}",
                changeType, solutionLabel, projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log change for project {ProjectId}", projectId);
            // Don't throw - logging failure shouldn't break the main operation
        }
    }

    public async Task<List<ChangeLog>> GetChangesAsync(Guid projectId, string? changeType = null, int limit = 100)
    {
        var query = _db.ChangeLogs
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId);

        if (!string.IsNullOrEmpty(changeType))
        {
            query = query.Where(c => c.ChangeType == changeType);
        }

        return await query
            .OrderByDescending(c => c.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ChangeLog>> GetChangesBySampleAsync(Guid projectId, string solutionLabel)
    {
        return await _db.ChangeLogs
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId && c.SolutionLabel == solutionLabel)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();
    }
}