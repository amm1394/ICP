using Core.Icp.Domain.Interfaces.Repositories;
using Core.Icp.Domain.Interfaces.Services;
using Core.Icp.Domain.Models.Files;
using Microsoft.AspNetCore.Mvc;
using Presentation.Icp.API.Models;
using Presentation.Icp.API.Models.Projects;

namespace Presentation.Icp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            IFileProcessingService fileProcessingService,
            IUnitOfWork unitOfWork,
            ILogger<ProjectsController> logger)
        {
            _fileProcessingService = fileProcessingService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
        [ProducesResponseType(typeof(ApiResponse<FileImportResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<FileImportResultDto>>> ImportProject(
            [FromForm] ProjectImportRequest request,
            CancellationToken cancellationToken)
        {
            var file = request.File;
            var projectName = request.ProjectName;

            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<FileImportResultDto>
                    .FailureResponse("فایلی ارسال نشده یا فایل خالی است."));
            }

            if (string.IsNullOrWhiteSpace(projectName))
            {
                return BadRequest(ApiResponse<FileImportResultDto>
                    .FailureResponse("نام پروژه الزامی است."));
            }

            var tempFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

            try
            {
                await using (var stream = System.IO.File.Create(tempFilePath))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                _logger.LogInformation(
                    "Importing project from file {FileName} to temp path {Path}",
                    file.FileName,
                    tempFilePath);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                ProjectImportResult importResult;

                if (extension == ".csv")
                {
                    importResult = await _fileProcessingService.ImportCsvAsync(
                        tempFilePath,
                        projectName,
                        cancellationToken);
                }
                else if (extension == ".xlsx" || extension == ".xls")
                {
                    importResult = await _fileProcessingService.ImportExcelAsync(
                        tempFilePath,
                        projectName,
                        null,
                        cancellationToken);
                }
                else
                {
                    return BadRequest(ApiResponse<FileImportResultDto>
                        .FailureResponse("نوع فایل پشتیبانی نمی‌شود. فقط CSV و Excel مجاز است."));
                }

                var project = importResult.Project;

                var resultDto = new FileImportResultDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    Success = importResult.Success,
                    Message = importResult.Message,
                    TotalRecords = importResult.TotalRecords,
                    SuccessfulRecords = importResult.SuccessfulRecords,
                    FailedRecords = importResult.FailedRecords,
                    TotalSamples = importResult.TotalSamples,
                    Errors = importResult.Errors,
                    Warnings = importResult.Warnings
                };

                return Ok(ApiResponse<FileImportResultDto>.SuccessResponse(resultDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while importing project from file {FileName}",
                    file.FileName);

                var errorResponse = ApiResponse<FileImportResultDto>.FailureResponse(
                    "خطای غیرمنتظره در هنگام ایمپورت پروژه رخ داد.");

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }


        #region Get Paged Projects

        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<ProjectSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedApiResponse<ProjectSummaryDto>>> GetProjects(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var (projects, totalCount) =
                await _unitOfWork.Projects.GetPagedAsync(pageNumber, pageSize, cancellationToken);

            var items = projects
                .Select(p => new ProjectSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt,
                    SampleCount = p.Samples?.Count ?? 0
                })
                .ToList();

            var response = PagedApiResponse<ProjectSummaryDto>.SuccessResponse(
                items,
                totalCount,
                pageNumber,
                pageSize,
                "فهرست پروژه‌ها با موفقیت بازیابی شد.");

            // توجه: TotalPages, HasPrevious, HasNext فقط getter هستند،
            // اینجا هیچ انتسابی به آن‌ها نداریم (اشکال CS0200 رفع می‌شود).
            return Ok(response);
        }

        #endregion

        #region Get Project By Id (with samples count)

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectSummaryDto>>> GetProjectById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.Projects
                .GetByIdWithSamplesAsync(id, cancellationToken);

            if (project == null)
            {
                return NotFound(ApiResponse<ProjectSummaryDto>
                    .FailureResponse("پروژه موردنظر یافت نشد."));
            }

            var dto = new ProjectSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status.ToString(),
                CreatedAt = project.CreatedAt,
                SampleCount = project.Samples?.Count ?? 0
            };

            return Ok(ApiResponse<ProjectSummaryDto>.SuccessResponse(dto));
        }

        #endregion
    }
}
