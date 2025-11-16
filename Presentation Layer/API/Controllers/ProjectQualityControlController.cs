using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Icp.API.Models;
using Shared.Icp.DTOs.QualityControl;
using Shared.Icp.Helpers.Mappers;

namespace Presentation.Icp.API.Controllers
{
    /// <summary>
    /// Endpointهای مربوط به کنترل کیفیت (QC) برای پروژه‌ها.
    /// </summary>
    [ApiController]
    [Route("api/projects/{projectId:guid}/qc")]
    [Produces("application/json")]
    public class ProjectQualityControlController : ControllerBase
    {
        private readonly IQualityControlService _qualityControlService;
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectQualityControlController> _logger;

        public ProjectQualityControlController(
            IQualityControlService qualityControlService,
            IProjectService projectService,
            ILogger<ProjectQualityControlController> logger)
        {
            _qualityControlService = qualityControlService;
            _projectService = projectService;
            _logger = logger;
        }

        #region Run checks

        [HttpPost("weight")]
        [ProducesResponseType(typeof(ApiResponse<List<QualityCheckResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<QualityCheckResultDto>>>> RunWeightChecks(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running weight QC for project {ProjectId}", projectId);

            var results = await _qualityControlService.RunWeightChecksAsync(projectId, cancellationToken);
            var dtoList = results.ToDtoList();

            var response = ApiResponse<List<QualityCheckResultDto>>
                .SuccessResponse(dtoList, "Weight QC executed successfully.");

            return Ok(response);
        }

        [HttpPost("volume")]
        [ProducesResponseType(typeof(ApiResponse<List<QualityCheckResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<QualityCheckResultDto>>>> RunVolumeChecks(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running volume QC for project {ProjectId}", projectId);

            var results = await _qualityControlService.RunVolumeChecksAsync(projectId, cancellationToken);
            var dtoList = results.ToDtoList();

            var response = ApiResponse<List<QualityCheckResultDto>>
                .SuccessResponse(dtoList, "Volume QC executed successfully.");

            return Ok(response);
        }

        [HttpPost("dilution")]
        [ProducesResponseType(typeof(ApiResponse<List<QualityCheckResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<QualityCheckResultDto>>>> RunDilutionFactorChecks(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dilution factor QC for project {ProjectId}", projectId);

            var results = await _qualityControlService.RunDilutionFactorChecksAsync(projectId, cancellationToken);
            var dtoList = results.ToDtoList();

            var response = ApiResponse<List<QualityCheckResultDto>>
                .SuccessResponse(dtoList, "Dilution factor QC executed successfully.");

            return Ok(response);
        }

        [HttpPost("empty")]
        [ProducesResponseType(typeof(ApiResponse<List<QualityCheckResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<QualityCheckResultDto>>>> RunEmptyChecks(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running empty QC for project {ProjectId}", projectId);

            var results = await _qualityControlService.RunEmptyChecksAsync(projectId, cancellationToken);
            var dtoList = results.ToDtoList();

            var response = ApiResponse<List<QualityCheckResultDto>>
                .SuccessResponse(dtoList, "Empty QC executed successfully.");

            return Ok(response);
        }

        [HttpPost("all")]
        [ProducesResponseType(typeof(ApiResponse<List<QualityCheckResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<QualityCheckResultDto>>>> RunAllChecks(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running all QC checks for project {ProjectId}", projectId);

            var results = await _qualityControlService.RunAllChecksAsync(projectId, cancellationToken);
            var dtoList = results.ToDtoList();

            var response = ApiResponse<List<QualityCheckResultDto>>
                .SuccessResponse(dtoList, "All QC checks executed successfully.");

            return Ok(response);
        }

        /// <summary>
        /// دریافت نتایج QC برای پروژه.
        /// اگر sampleId داده شود، فقط QCهای همان نمونه برگردانده می‌شود.
        /// </summary>
        [HttpGet("checks")]
        [ProducesResponseType(typeof(ApiResponse<List<QualityCheckResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<QualityCheckResultDto>>>> GetAllChecks(
            Guid projectId,
            [FromQuery] Guid? sampleId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Retrieving QC checks for project {ProjectId}, sampleId = {SampleId}",
                projectId,
                sampleId);

            var results = await _qualityControlService.RunAllChecksAsync(projectId, cancellationToken);

            if (sampleId.HasValue && sampleId.Value != Guid.Empty)
            {
                results = results
                    .Where(r => r.SampleId == sampleId.Value)
                    .ToList();
            }

            var dtoList = results.ToDtoList();

            var response = ApiResponse<List<QualityCheckResultDto>>
                .SuccessResponse(dtoList, "QC checks retrieved successfully.");

            return Ok(response);
        }

        #endregion

        #region Summary

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ProjectQualitySummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ProjectQualitySummaryDto>>> GetProjectQualitySummary(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving QC summary for project {ProjectId}", projectId);

            var summary = await _qualityControlService.GetProjectQualitySummaryAsync(projectId, cancellationToken);
            var dto = summary.ToDto();

            var response = ApiResponse<ProjectQualitySummaryDto>
                .SuccessResponse(dto, "QC summary retrieved successfully.");

            return Ok(response);
        }

        #endregion

        #region Settings

        [HttpGet("settings")]
        [ProducesResponseType(typeof(ApiResponse<ProjectQualitySettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectQualitySettingsDto>>> GetQualitySettings(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving QC settings for project {ProjectId}", projectId);

            var settings = await _projectService.GetProjectSettingsAsync(projectId, cancellationToken);
            if (settings == null)
            {
                return NotFound(ApiResponse<ProjectQualitySettingsDto>
                    .FailureResponse("پروژه یافت نشد."));
            }

            var dto = settings.ToQualitySettingsDto(projectId);

            return Ok(ApiResponse<ProjectQualitySettingsDto>
                .SuccessResponse(dto, "QC settings retrieved successfully."));
        }

        [HttpPut("settings")]
        [ProducesResponseType(typeof(ApiResponse<ProjectQualitySettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProjectQualitySettingsDto>>> UpdateQualitySettings(
            Guid projectId,
            [FromBody] ProjectQualitySettingsDto dto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating QC settings for project {ProjectId}", projectId);

            var existingSettings = await _projectService.GetProjectSettingsAsync(projectId, cancellationToken);
            if (existingSettings == null)
            {
                return NotFound(ApiResponse<ProjectQualitySettingsDto>
                    .FailureResponse("پروژه یافت نشد."));
            }

            existingSettings.ApplyFromDto(dto);

            var updated = await _projectService.UpdateProjectSettingsAsync(
                projectId,
                existingSettings,
                cancellationToken);

            var resultDto = updated.ToQualitySettingsDto(projectId);

            return Ok(ApiResponse<ProjectQualitySettingsDto>
                .SuccessResponse(resultDto, "QC settings updated successfully."));
        }

        #endregion
    }
}
