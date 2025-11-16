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

                // بعد از این‌که importResult پر شد:
                var resultDto = new FileImportResultDto
                {
                    ProjectId = importResult.Project.Id,
                    ProjectName = importResult.Project.Name ?? string.Empty,

                    Success = importResult.FailedRecords == 0 && importResult.Errors.Count == 0,

                    Message = importResult.FailedRecords == 0
                        ? "ایمپورت با موفقیت انجام شد."
                        : "ایمپورت با هشدار/خطا انجام شد.",

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
    }
}
