using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Infrastructure.Services
{
    /// <summary>
    /// Background service that processes import jobs enqueued by the API.
    /// - Persists ProjectImportJobs records in DB (created when enqueued).
    /// - Maintains an in-memory status map (_statuses) for quick reads.
    /// - Uses IImportService to perform the actual import and reports progress back to DB.
    /// 
    /// Implements Application.Services.IImportQueueService so it can be registered and resolved by DI.
    /// </summary>
    public class BackgroundImportQueueService : BackgroundService, IImportQueueService, IDisposable
    {
        private readonly Channel<ImportRequest> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundImportQueueService> _logger;
        private readonly ConcurrentDictionary<Guid, Shared.Models.ImportJobStatusDto> _statuses;

        public BackgroundImportQueueService(IServiceProvider serviceProvider, ILogger<BackgroundImportQueueService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _channel = Channel.CreateUnbounded<ImportRequest>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
            _statuses = new ConcurrentDictionary<Guid, Shared.Models.ImportJobStatusDto>();
        }

        /// <summary>
        /// Enqueue a stream containing CSV content for import.
        /// The implementation copies the provided stream into a MemoryStream so the caller can dispose theirs.
        /// Returns the created jobId.
        /// </summary>
        public async Task<Guid> EnqueueImportAsync(Stream csvStream, string projectName, string? owner = null, string? stateJson = null)
        {
            if (csvStream == null) throw new ArgumentNullException(nameof(csvStream));
            var jobId = Guid.NewGuid();

            // Persist job record in DB
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IsatisDbContext>();

                var entity = new ProjectImportJob
                {
                    JobId = jobId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProjectName = projectName ?? "ImportedProject",
                    State = (int)Shared.Models.ImportJobState.Pending,
                    Percent = 0,
                    ProcessedRows = 0,
                    TotalRows = 0,
                    Message = "Queued",

                    // Initialize idempotency/retry fields (migration must exist)
                    OperationId = Guid.NewGuid(),
                    Attempts = 0,
                    LastError = null,
                    NextAttemptAt = null
                };

                db.ProjectImportJobs.Add(entity);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist enqueue job {JobId}", jobId);
                throw;
            }

            // copy Stream to a MemoryStream so we own the lifetime and can dispose later
            var copy = new MemoryStream();
            try
            {
                if (csvStream.CanSeek) csvStream.Position = 0;
            }
            catch
            {
                // ignore if stream is not seekable
            }

            await csvStream.CopyToAsync(copy);
            copy.Position = 0;

            var req = new ImportRequest
            {
                JobId = jobId,
                Stream = copy,
                ProjectName = projectName,
                Owner = owner,
                StateJson = stateJson
            };

            // initial in-memory status
            _statuses[jobId] = new Shared.Models.ImportJobStatusDto(jobId, Shared.Models.ImportJobState.Pending, 0, 0, "Queued", null, 0);

            // enqueue
            await _channel.Writer.WriteAsync(req);

            return jobId;
        }

        /// <summary>
        /// Get status for a job (in-memory quick lookup; falls back to DB if missing).
        /// </summary>
        public async Task<Shared.Models.ImportJobStatusDto?> GetStatusAsync(Guid jobId)
        {
            if (_statuses.TryGetValue(jobId, out var status)) return status;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IsatisDbContext>();
                var entity = await db.ProjectImportJobs.FindAsync(jobId);
                if (entity == null) return null;

                var st = Shared.Models.ImportJobState.Pending;
                if (entity.State == (int)Shared.Models.ImportJobState.Running) st = Shared.Models.ImportJobState.Running;
                else if (entity.State == (int)Shared.Models.ImportJobState.Completed) st = Shared.Models.ImportJobState.Completed;
                else if (entity.State == (int)Shared.Models.ImportJobState.Failed) st = Shared.Models.ImportJobState.Failed;

                var dto = new Shared.Models.ImportJobStatusDto(entity.JobId, st, entity.TotalRows, entity.ProcessedRows, entity.Message, entity.ResultProjectId, entity.Percent);
                _statuses[jobId] = dto;
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read job status from DB for {JobId}", jobId);
                return null;
            }
        }

        /// <summary>
        /// Background worker loop: consumes queued import requests and processes them sequentially.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundImportQueueService started.");
            await foreach (var req in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested) break;
                try
                {
                    await ProcessRequestAsync(req, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Processing cancelled for job {JobId}", req.JobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception while processing import job {JobId}", req.JobId);

                    // persist failed state
                    try
                    {
                        using var scope2 = _serviceProvider.CreateScope();
                        var db2 = scope2.ServiceProvider.GetRequiredService<IsatisDbContext>();
                        var entity2 = await db2.ProjectImportJobs.FindAsync(req.JobId);
                        if (entity2 != null)
                        {
                            entity2.State = (int)Shared.Models.ImportJobState.Failed;
                            entity2.Message = ex.Message;
                            entity2.UpdatedAt = DateTime.UtcNow;
                            entity2.Attempts += 1;
                            entity2.LastError = ex.Message;
                            db2.ProjectImportJobs.Update(entity2);
                            await db2.SaveChangesAsync();
                        }
                    }
                    catch (Exception inner)
                    {
                        _logger.LogWarning(inner, "Failed to persist failure for job {JobId} after exception", req.JobId);
                    }

                    _statuses[req.JobId] = new Shared.Models.ImportJobStatusDto(req.JobId, Shared.Models.ImportJobState.Failed, 0, 0, ex.Message, null, 0);
                }
                finally
                {
                    // ensure stream disposed
                    try { req.Stream.Dispose(); } catch { }
                }
            }

            _logger.LogInformation("BackgroundImportQueueService stopping.");
        }

        private async Task ProcessRequestAsync(ImportRequest req, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing import job {JobId} (Project: {ProjectName})", req.JobId, req.ProjectName);

            // update DB to Running
            ProjectImportJob? entity0 = null;
            try
            {
                using var scope0 = _serviceProvider.CreateScope();
                var db0 = scope0.ServiceProvider.GetRequiredService<IsatisDbContext>();
                entity0 = await db0.ProjectImportJobs.FindAsync(req.JobId);
                if (entity0 != null)
                {
                    entity0.State = (int)Shared.Models.ImportJobState.Running;
                    entity0.UpdatedAt = DateTime.UtcNow;
                    entity0.Message = "Running";
                    db0.ProjectImportJobs.Update(entity0);
                    await db0.SaveChangesAsync();
                }

                _statuses[req.JobId] = new Shared.Models.ImportJobStatusDto(req.JobId, Shared.Models.ImportJobState.Running, entity0?.TotalRows ?? 0, entity0?.ProcessedRows ?? 0, "Running", null, entity0?.Percent ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist job state=Running for job {JobId}", req.JobId);
            }

            // Use ImportService from scoped provider
            using var scope = _serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IImportService>();
            var db = scope.ServiceProvider.GetRequiredService<IsatisDbContext>();

            // Progress reporter: persist periodic progress back to DB and update in-memory status
            var progress = new Progress<(int total, int processed)>(async t =>
            {
                try
                {
                    var (totalRows, processedRows) = t;
                    var percent = totalRows == 0 ? 0 : (int)Math.Round((processedRows / (double)totalRows) * 100.0);

                    using var scopeP = _serviceProvider.CreateScope();
                    var dbP = scopeP.ServiceProvider.GetRequiredService<IsatisDbContext>();
                    var entityP = await dbP.ProjectImportJobs.FindAsync(req.JobId);
                    if (entityP != null)
                    {
                        entityP.ProcessedRows = processedRows;
                        entityP.TotalRows = totalRows;
                        entityP.Percent = percent;
                        entityP.UpdatedAt = DateTime.UtcNow;
                        entityP.Message = "Running";
                        entityP.Attempts = Math.Max(entityP.Attempts, 0);
                        dbP.ProjectImportJobs.Update(entityP);
                        await dbP.SaveChangesAsync();
                    }

                    _statuses[req.JobId] = new Shared.Models.ImportJobStatusDto(req.JobId, Shared.Models.ImportJobState.Running, totalRows, processedRows, "Running", null, percent);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist progress for job {JobId}", req.JobId);
                }
            });

            // Ensure stream position at start
            try { req.Stream.Position = 0; } catch { }

            // Call import service using stream
            var result = await importService.ImportCsvAsync(req.Stream, req.ProjectName ?? "ImportedProject", req.Owner, req.StateJson, progress);

            if (result.Succeeded)
            {
                var projectId = result.Data?.ProjectId;

                // update DB record final state
                try
                {
                    var entity = await db.ProjectImportJobs.FindAsync(req.JobId);
                    if (entity != null)
                    {
                        entity.ResultProjectId = projectId;
                        entity.State = (int)Shared.Models.ImportJobState.Completed;
                        entity.Percent = 100;
                        entity.ProcessedRows = entity.TotalRows; // best-effort
                        entity.UpdatedAt = DateTime.UtcNow;
                        entity.Message = "Completed";
                        db.ProjectImportJobs.Update(entity);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist completion for job {JobId}", req.JobId);
                }

                _statuses[req.JobId] = new Shared.Models.ImportJobStatusDto(req.JobId, Shared.Models.ImportJobState.Completed, _statuses[req.JobId].TotalRows, _statuses[req.JobId].ProcessedRows, "Completed", projectId, 100);
                _logger.LogInformation("Import job {JobId} completed. ProjectId: {ProjectId}", req.JobId, projectId);
            }
            else
            {
                // SAFE access to result.Messages (coalesce to avoid null-ref warnings)
                var msg = (result.Messages ?? Array.Empty<string>()).FirstOrDefault() ?? "Import failed";

                // persist failure
                try
                {
                    var entity = await db.ProjectImportJobs.FindAsync(req.JobId);
                    if (entity != null)
                    {
                        entity.State = (int)Shared.Models.ImportJobState.Failed;
                        entity.Message = msg;
                        entity.UpdatedAt = DateTime.UtcNow;
                        entity.Attempts += 1;
                        entity.LastError = msg;
                        db.ProjectImportJobs.Update(entity);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist failure for job {JobId}", req.JobId);
                }

                _statuses[req.JobId] = new Shared.Models.ImportJobStatusDto(req.JobId, Shared.Models.ImportJobState.Failed, _statuses[req.JobId].TotalRows, _statuses[req.JobId].ProcessedRows, msg, null, _statuses[req.JobId].Percent);
                _logger.LogWarning("Import job {JobId} failed: {Msg}", req.JobId, msg);
            }
        }

        public override void Dispose()
        {
            try
            {
                _channel.Writer.Complete();
            }
            catch { }
            base.Dispose();
        }

        private sealed class ImportRequest
        {
            public Guid JobId { get; set; }
            public MemoryStream Stream { get; set; } = null!;
            public string? ProjectName { get; set; }
            public string? Owner { get; set; }
            public string? StateJson { get; set; }
        }
    }
}