using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Isatis.Api.Controllers;

// This controller provides synchronous import functionality
// The new Api.Controllers.ProjectsController provides async/background import at /api/projects/import
[ApiController]
[Route("api/projects")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly IImportQueueService _importQueue;

    public ImportController(IImportService importService, IImportQueueService importQueue)
    {
        _importService = importService;
        _importQueue = importQueue;
    }

    /// Import CSV synchronously (alternative endpoint to avoid conflict with ProjectsController)
    [HttpPost("import-sync")]
    [RequestSizeLimit(200 * 1024 * 1024)]
    public async Task<ActionResult<Result<object>>> ImportCsvSync([FromForm] IFormFile? file, [FromForm] string? projectName, [FromForm] string? owner, [FromForm] string? stateJson)
    {
        if (file == null) return BadRequest(Result<object>.Fail("File is required"));
        if (file.Length == 0) return BadRequest(Result<object>.Fail("File is empty"));

        // default project name
        projectName ??= "ImportedProject";

        using var stream = file.OpenReadStream();
        var res = await _importService.ImportCsvAsync(stream, projectName, owner, stateJson);
        if (res.Succeeded) return Ok(Result<object>.Success(new { ProjectId = res.Data!.ProjectId }));
        var firstMsg = (res.Messages ?? Array.Empty<string>()).FirstOrDefault();
        return BadRequest(Result<object>.Fail(firstMsg ?? "Import failed"));
    }
}