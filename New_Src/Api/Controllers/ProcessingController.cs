using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Isatis.Api.Controllers;

// This controller provides processing functionality with sync support
// The new Api.Controllers.ProjectsController provides background-only processing
[ApiController]
[Route("api/projects")]
public class ProcessingController : ControllerBase
{
    private readonly IProcessingService _processingService;

    public ProcessingController(IProcessingService processingService)
    {
        _processingService = processingService;
    }

    // POST /api/projects/{projectId}/process-sync
    // Synchronous processing endpoint (to avoid conflict with ProjectsController)
    [HttpPost("{projectId:guid}/process-sync")]
    public async Task<ActionResult<Result<object>>> ProcessProjectSync(Guid projectId)
    {
        var resSync = await _processingService.ProcessProjectAsync(projectId);
        if (resSync.Succeeded) return Ok(Result<object>.Success(new { ProjectStateId = resSync.Data }));
        var firstMsgSync = (resSync.Messages ?? Array.Empty<string>()).FirstOrDefault();
        return BadRequest(Result<object>.Fail(firstMsgSync ?? "Processing failed"));
    }
}