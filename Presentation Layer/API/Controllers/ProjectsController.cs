using Core.Icp.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Presentation.Icp.API.Models;
using Shared.Icp.DTOs.Projects;
using Shared.Icp.Helpers.Mappers;

namespace Presentation.Icp.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing analysis projects.
    /// </summary>
    /// <remarks>
    /// Responses are wrapped in <see cref="ApiResponse{T}"/> and پیام‌های کاربر به فارسی هستند.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        /// <summary>
        /// سرویس پروژه‌ها (لایه Application / Domain).
        /// </summary>
        private readonly IProjectService _projectService;

        /// <summary>
        /// Logger instance for diagnostic and audit logging.
        /// </summary>
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            IProjectService projectService,
            ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// لیست همه پروژه‌ها (خروجی Summary).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ProjectSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ProjectSummaryDto>>>> GetAll()
        {
            _logger.LogInformation("Getting all projects");

            var projects = await _projectService.GetAllProjectsAsync();
            var projectDtos = projects.ToSummaryDtoList();

            return Ok(ApiResponse<List<ProjectSummaryDto>>.SuccessResponse(
                projectDtos,
                $"{projectDtos.Count} پروژه یافت شد"));
        }

        /// <summary>
        /// دریافت یک پروژه به‌همراه جزئیات.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid id)
        {
            _logger.LogInformation("Getting project {ProjectId}", id);

            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
                return NotFound(ApiResponse<ProjectDto>.FailureResponse("پروژه یافت نشد"));

            var projectDto = project.ToDto();
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(projectDto));
        }

        /// <summary>
        /// ایجاد پروژه جدید.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> Create([FromBody] CreateProjectDto dto)
        {
            _logger.LogInformation("Creating new project: {ProjectName}", dto.Name);

            var project = dto.ToEntity();
            project = await _projectService.CreateProjectAsync(project);

            var projectDto = project.ToDto();

            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                ApiResponse<ProjectDto>.SuccessResponse(projectDto, "پروژه با موفقیت ایجاد شد"));
        }

        /// <summary>
        /// ویرایش پروژه.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(
            Guid id,
            [FromBody] UpdateProjectDto dto)
        {
            _logger.LogInformation("Updating project {ProjectId}", id);

            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
                return NotFound(ApiResponse<ProjectDto>.FailureResponse("پروژه یافت نشد"));

            // مپ شدن فیلدها طبق Mapper
            project.UpdateFromDto(dto);

            project = await _projectService.UpdateProjectAsync(project);

            var projectDto = project.ToDto();
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(projectDto, "پروژه با موفقیت به‌روزرسانی شد"));
        }

        /// <summary>
        /// حذف نرم پروژه.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting project {ProjectId}", id);

            var deleted = await _projectService.DeleteProjectAsync(id);

            if (!deleted)
                return NotFound(ApiResponse<object>.FailureResponse("پروژه یافت نشد"));

            return NoContent();
        }

        /// <summary>
        /// آخرین پروژه‌های ایجاد شده.
        /// </summary>
        [HttpGet("recent/{count:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<ProjectSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ProjectSummaryDto>>>> GetRecent(int count = 10)
        {
            _logger.LogInformation("Getting {Count} recent projects", count);

            var projects = await _projectService.GetRecentProjectsAsync(count);
            var projectDtos = projects.ToSummaryDtoList();

            return Ok(ApiResponse<List<ProjectSummaryDto>>.SuccessResponse(projectDtos));
        }
    }
}
