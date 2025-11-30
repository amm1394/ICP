// NOTE: This is the same ImportService file with safe use of Messages.FirstOrDefault().
// I only show the full file or relevant region depending on repo layout; here is the updated content region shown previously.
using Application.Services;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Wrapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ImportService : IImportService
    {
        private const int DefaultChunkSize = 500;
        private readonly IProjectPersistenceService _persistence;
        private readonly ILogger<ImportService> _logger;

        public ImportService(IProjectPersistenceService persistence, ILogger<ImportService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<Result<ProjectSaveResult>> ImportCsvAsync(Stream stream, string projectName, string? owner, string? stateJson, IProgress<(int total, int processed)>? progress = null)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                // Count rows
                int totalRows = 0;
                using (var readerCount = new StreamReader(ms, leaveOpen: true))
                using (var csvCount = new CsvReader(readerCount, CultureInfo.InvariantCulture))
                {
                    if (!csvCount.Read()) return Result<ProjectSaveResult>.Fail("CSV empty");
                    csvCount.ReadHeader();
                    while (csvCount.Read())
                    {
                        totalRows++;
                    }
                }

                // Reset to start for actual processing
                ms.Position = 0;

                // Second pass: process in chunks and report progress
                using var reader = new StreamReader(ms);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    DetectDelimiter = true,
                    BadDataFound = null,
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var csv = new CsvReader(reader, config);
                if (!csv.Read()) return Result<ProjectSaveResult>.Fail("CSV empty");
                csv.ReadHeader();
                var headers = csv.HeaderRecord;
                if (headers == null || headers.Length == 0) return Result<ProjectSaveResult>.Fail("CSV has no header");

                var processed = 0;
                Guid? knownProjectId = null;
                var batch = new List<RawDataDto>(DefaultChunkSize);

                while (csv.Read())
                {
                    string? sampleId = null;
                    var sampleIdHeader = headers.FirstOrDefault(h => string.Equals(h, "SampleId", StringComparison.OrdinalIgnoreCase));
                    if (sampleIdHeader != null)
                    {
                        sampleId = csv.GetField(sampleIdHeader);
                    }

                    var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var h in headers)
                    {
                        if (string.Equals(h, "SampleId", StringComparison.OrdinalIgnoreCase)) continue;
                        var value = csv.GetField(h);
                        if (value == null) { dict[h] = null; continue; }
                        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) dict[h] = d;
                        else if (bool.TryParse(value, out var b)) dict[h] = b;
                        else dict[h] = value;
                    }

                    var columnDataJson = JsonSerializer.Serialize(dict);
                    batch.Add(new RawDataDto(columnDataJson, string.IsNullOrWhiteSpace(sampleId) ? null : sampleId));
                    processed++;

                    // report intermediate progress if desired
                    progress?.Report((totalRows, processed));

                    if (batch.Count >= DefaultChunkSize)
                    {
                        // save batch - if we already have project id use it, otherwise pass Guid.Empty
                        var saveProjectId = knownProjectId ?? Guid.Empty;
                        var res = await _persistence.SaveProjectAsync(saveProjectId, projectName, owner, batch, stateJson);
                        if (!res.Succeeded)
                        {
                            var msg = (res.Messages ?? Array.Empty<string>()).FirstOrDefault();
                            return Result<ProjectSaveResult>.Fail($"Import failed during batch save: {msg ?? "unknown error"}");
                        }

                        knownProjectId = res.Data!.ProjectId;
                        batch.Clear();
                    }
                }

                // save remaining
                if (batch.Count > 0)
                {
                    var saveProjectId = knownProjectId ?? Guid.Empty;
                    var res = await _persistence.SaveProjectAsync(saveProjectId, projectName, owner, batch, stateJson);
                    if (!res.Succeeded)
                    {
                        var msg = (res.Messages ?? Array.Empty<string>()).FirstOrDefault();
                        return Result<ProjectSaveResult>.Fail($"Import failed during final save: {msg ?? "unknown error"}");
                    }

                    knownProjectId = res.Data!.ProjectId;
                    batch.Clear();
                }

                // final report
                progress?.Report((totalRows, processed));

                return Result<ProjectSaveResult>.Success(new ProjectSaveResult(knownProjectId ?? Guid.Empty));
            }
            catch (Exception ex)
            {
                return Result<ProjectSaveResult>.Fail($"Import failed: {ex.Message}");
            }
            finally
            {
                try { ms.Dispose(); } catch { }
            }
        }
    }
}