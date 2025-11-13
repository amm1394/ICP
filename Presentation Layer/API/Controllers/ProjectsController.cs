using Core.Icp.Domain.Interfaces.Repositories;
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
    /// This controller exposes CRUD operations and utility endpoints for projects, including
    /// retrieval of summaries and recent items. Responses are wrapped in a consistent
    /// <see cref="ApiResponse{T}"/> envelope. User-facing messages in responses remain localized in Persian.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        /// <summary>
        /// Unit of Work for accessing repositories and persisting changes.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Logger instance for diagnostic and audit logging.
        /// </summary>
        private readonly ILogger<ProjectsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectsController"/> class.
        /// </summary>
        /// <param name="unitOfWork">Unit of Work for accessing repositories and persisting changes.</param>
        /// <param name="logger">The logger instance.</param>
        public ProjectsController(IUnitOfWork unitOfWork, ILogger<ProjectsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all projects as summaries.
        /// </summary>
        /// <returns>
        /// 200 OK with a list of <see cref="ProjectSummaryDto"/> wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ProjectSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ProjectSummaryDto>>>> GetAll()
        {
            _logger.LogInformation("Getting all projects");

            var projects = await _unitOfWork.Projects.GetAllAsync();
            var projectDtos = projects.ToSummaryDtoList();

            return Ok(ApiResponse<List<ProjectSummaryDto>>.SuccessResponse(
                projectDtos,
                $"{projectDtos.Count} پروژه یافت شد"));
        }

        /// <summary>
        /// Gets a project by its unique identifier, including its samples.
        /// </summary>
        /// <param name="id">The project identifier.</param>
        /// <returns>
        /// 200 OK with a <see cref="ProjectDto"/> wrapped in <see cref="ApiResponse{T}"/> when found;
        /// 404 Not Found when the project does not exist.
        /// </returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid id)
        {
            _logger.LogInformation("Getting project {ProjectId}", id);

            var project = await _unitOfWork.Projects.GetWithSamplesAsync(id);

            if (project == null)
                return NotFound(ApiResponse<ProjectDto>.FailureResponse("پروژه یافت نشد"));

            var projectDto = project.ToDto();
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(projectDto));
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="dto">The request payload containing project data.</param>
        /// <returns>
        /// 201 Created with the created <see cref="ProjectDto"/> wrapped in <see cref="ApiResponse{T}"/>;
        /// 400 Bad Request if the payload is invalid.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> Create([FromBody] CreateProjectDto dto)
        {
            _logger.LogInformation("Creating new project: {ProjectName}", dto.Name);

            var project = dto.ToEntity();
            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            var projectDto = project.ToDto();

            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                ApiResponse<ProjectDto>.SuccessResponse(projectDto, "پروژه با موفقیت ایجاد شد"));
        }

        /// <summary>
        /// Updates an existing project.
        /// </summary>
        /// <param name="id">The identifier of the project to update.</param>
        /// <param name="dto">The payload containing updated values.</param>
        /// <returns>
        /// 200 OK with the updated <see cref="ProjectDto"/> wrapped in <see cref="ApiResponse{T}"/>;
        /// 404 Not Found when the project does not exist.
        /// </returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(Guid id, [FromBody] UpdateProjectDto dto)
        {
            _logger.LogInformation("Updating project {ProjectId}", id);

            var project = await _unitOfWork.Projects.GetByIdAsync(id);

            if (project == null)
                return NotFound(ApiResponse<ProjectDto>.FailureResponse("پروژه یافت نشد"));

            project.UpdateFromDto(dto);
            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            var projectDto = project.ToDto();
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(projectDto, "پروژه با موفقیت به‌روزرسانی شد"));
        }

        /// <summary>
        /// Deletes a project using soft delete semantics.
        /// </summary>
        /// <param name="id">The identifier of the project to delete.</param>
        /// <returns>
        /// 204 No Content on success;
        /// 404 Not Found when the project does not exist.
        /// </returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting project {ProjectId}", id);

            var project = await _unitOfWork.Projects.GetByIdAsync(id);

            if (project == null)
                return NotFound(ApiResponse<object>.FailureResponse("پروژه یافت نشد"));

            await _unitOfWork.Projects.DeleteAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Gets the most recent projects as summaries.
        /// </summary>
        /// <param name="count">The number of recent projects to return. Defaults to 10.</param>
        /// <returns>
        /// 200 OK with a list of <see cref="ProjectSummaryDto"/> wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        [HttpGet("recent/{count:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<ProjectSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ProjectSummaryDto>>>> GetRecent(int count = 10)
        {
            _logger.LogInformation("Getting {Count} recent projects", count);

            var projects = await _unitOfWork.Projects.GetRecentProjectsAsync(count);
            var projectDtos = projects.ToSummaryDtoList();

            return Ok(ApiResponse<List<ProjectSummaryDto>>.SuccessResponse(projectDtos));
        }
    }
}