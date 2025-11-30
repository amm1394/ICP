using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Application.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IImportQueueService _queue;
        private readonly Application.Services.IProjectPersistenceService _persistence;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IImportQueueService queue, Application.Services.IProjectPersistenceService persistence, ILogger<ProjectsController> logger)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // POST api/projects/{projectId}/process? background=true
        [HttpPost("{projectId:guid}/process")]
        public async Task<IActionResult> EnqueueProcess([FromRoute] Guid projectId, [FromQuery] bool background = true)
        {
            if (projectId == Guid.Empty)
                return BadRequest(new ApiResponse<object>(false, null, new[] { "projectId is required" }));

            try
            {
                var project = await TryGetProjectAsync(projectId, HttpContext.RequestAborted);
                if (project == null)
                    return NotFound(new ApiResponse<object>(false, null, new[] { "Project not found." }));

                if (background)
                {
                    var jobId = await _queue.EnqueueProcessJobAsync(projectId);
                    return Ok(new ApiResponse<object>(true, new { jobId }, Array.Empty<string>()));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, null, new[] { "Synchronous processing not supported.  Use ? background=true." }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue process for project {ProjectId}", projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, new[] { ex.Message }));
            }
        }

        // NOTE: Import endpoint moved to ImportController to avoid duplicate routes
        // Use POST /api/projects/import from ImportController instead

        // GET api/projects/import/{jobId:guid}/status
        [HttpGet("import/{jobId:guid}/status")]
        public async Task<IActionResult> GetJobStatus([FromRoute] Guid jobId)
        {
            if (jobId == Guid.Empty)
                return BadRequest(new ApiResponse<object>(false, null, new[] { "jobId is required" }));

            try
            {
                var status = await _queue.GetStatusAsync(jobId);
                if (status == null)
                    return NotFound(new ApiResponse<object>(false, null, new[] { "Job not found." }));

                return Ok(new ApiResponse<object>(true, status, Array.Empty<string>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get job status for {JobId}", jobId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, new[] { ex.Message }));
            }
        }

        // GET api/projects/{projectId}/load
        [HttpGet("{projectId:guid}/load")]
        public async Task<IActionResult> LoadProject([FromRoute] Guid projectId)
        {
            if (projectId == Guid.Empty)
                return BadRequest(new ApiResponse<object>(false, null, new[] { "projectid is required" }));

            try
            {
                var project = await TryGetProjectAsync(projectId, HttpContext.RequestAborted);
                if (project == null)
                    return NotFound(new ApiResponse<object>(false, null, new[] { "Project not found." }));

                return Ok(new ApiResponse<object>(true, project, Array.Empty<string>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load project {ProjectId}", projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, new[] { ex.Message }));
            }
        }

        // Helper: attempt to call any reasonable "get project" method on the persistence service via reflection.
        private async Task<object?> TryGetProjectAsync(Guid projectId, CancellationToken cancellationToken)
        {
            if (_persistence == null) return null;

            var svc = _persistence;
            var svcType = svc.GetType();

            var candidateNames = new[]
            {
                "GetProjectAsync", "GetAsync", "LoadProjectAsync", "LoadAsync", "FindProjectAsync",
                "GetByIdAsync", "FindAsync", "GetProject", "Get", "Find"
            };

            foreach (var name in candidateNames)
            {
                var methods = svcType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                     .Where(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase))
                                     .ToArray();

                foreach (var m in methods)
                {
                    var parameters = m.GetParameters();
                    if (parameters.Length == 0) continue;

                    var firstParam = parameters[0].ParameterType;
                    if (firstParam != typeof(Guid) && firstParam != typeof(Guid?) && firstParam != typeof(string)) continue;

                    object?[] args;
                    if (parameters.Length == 1)
                    {
                        args = new object?[] { projectId };
                    }
                    else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(CancellationToken))
                    {
                        args = new object?[] { projectId, cancellationToken };
                    }
                    else
                    {
                        continue;
                    }

                    try
                    {
                        var invoked = m.Invoke(svc, args);
                        if (invoked == null) return null;

                        if (invoked is Task task)
                        {
                            await task.ConfigureAwait(false);
                            var resultProp = task.GetType().GetProperty("Result");
                            return resultProp?.GetValue(task);
                        }
                        else
                        {
                            return invoked;
                        }
                    }
                    catch (TargetInvocationException tie)
                    {
                        _logger.LogWarning(tie, "Reflection invocation of {Method} failed", m.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Reflection invocation of {Method} failed", m.Name);
                    }
                }
            }

            _logger.LogDebug("TryGetProjectAsync: no suitable method found on {Type}", svcType.FullName);
            return null;
        }
    }
}