using Core.Icp.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Presentation.Icp.API.Models;
using Shared.Icp.DTOs.Projects;
using Shared.Icp.Helpers.Mappers;

namespace Presentation.Icp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IUnitOfWork unitOfWork, ILogger<ProjectsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// دریافت لیست تمام پروژه‌ها
        /// </summary>
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
        /// دریافت یک پروژه با شناسه
        /// </summary>
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
        /// ایجاد پروژه جدید
        /// </summary>
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
        /// به‌روزرسانی پروژه
        /// </summary>
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
        /// حذف پروژه (Soft Delete)
        /// </summary>
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
        /// دریافت پروژه‌های اخیر
        /// </summary>
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