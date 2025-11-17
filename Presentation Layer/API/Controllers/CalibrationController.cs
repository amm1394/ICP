using Core.Icp.Application.Interfaces;
using Core.Icp.Application.Models.Calibration;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers // اگر کنترلرهای دیگر namespace دیگری دارند، این را هماهنگ کن
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalibrationController : ControllerBase
    {
        private readonly ICalibrationService _calibrationService;

        public CalibrationController(ICalibrationService calibrationService)
        {
            _calibrationService = calibrationService;
        }

        [HttpPost]
        public async Task<ActionResult<CalibrationCurveDto>> CreateCurve(
            [FromBody] CreateCalibrationCurveDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _calibrationService.CreateCurveAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CalibrationCurveDto>> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var curve = await _calibrationService.GetByIdAsync(id, cancellationToken);
            if (curve == null)
                return NotFound();

            return Ok(curve);
        }

        [HttpGet("project/{projectId:guid}")]
        public async Task<ActionResult<IReadOnlyCollection<CalibrationCurveDto>>> GetForProject(
            Guid projectId,
            [FromQuery] Guid? elementId = null,
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var curves = await _calibrationService.GetCurvesForProjectAsync(
                projectId, elementId, onlyActive, cancellationToken);

            return Ok(curves);
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await _calibrationService.DeactivateCurveAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("evaluate")]
        public async Task<ActionResult<decimal?>> Evaluate(
            [FromQuery] Guid projectId,
            [FromQuery] Guid elementId,
            [FromQuery] decimal intensity,
            CancellationToken cancellationToken = default)
        {
            var result = await _calibrationService.EvaluateConcentrationAsync(
                projectId, elementId, intensity, cancellationToken);

            if (result is null)
                return NotFound("No active calibration curve found for this project/element.");

            return Ok(result);
        }

        /// <summary>
        /// خلاصه کالیبراسیون برای یک پروژه و عنصر مشخص
        /// </summary>
        [HttpGet("project/{projectId:guid}/summary")]
        public async Task<ActionResult<CalibrationSummaryDto>> GetSummary(
            Guid projectId,
            [FromQuery] Guid elementId,
            CancellationToken cancellationToken = default)
        {
            var summary = await _calibrationService.GetCalibrationSummaryAsync(
                projectId, elementId, cancellationToken);

            if (summary == null)
                return NotFound("No active calibration curve found for this project/element.");

            return Ok(summary);
        }

        /// <summary>
        /// QC یک نمونه CRM/RM بر اساس منحنی فعال پروژه/عنصر
        /// </summary>
        [HttpGet("qc")]
        public async Task<ActionResult<QcCheckResultDto>> EvaluateQc(
            [FromQuery] Guid projectId,
            [FromQuery] Guid elementId,
            [FromQuery] decimal expectedConcentration,
            [FromQuery] decimal intensity,
            [FromQuery] decimal tolerancePercent,
            CancellationToken cancellationToken = default)
        {
            var result = await _calibrationService.EvaluateQcAsync(
                projectId,
                elementId,
                expectedConcentration,
                intensity,
                tolerancePercent,
                cancellationToken);

            if (result is null)
                return NotFound("No active calibration curve found for this project/element.");

            return Ok(result);
        }

        /// <summary>
        /// اعمال کالیبراسیون روی چند نمونه (Intensity → Concentration)
        /// </summary>
        [HttpPost("apply")]
        public async Task<ActionResult<IReadOnlyCollection<CalibratedSampleResultDto>>> Apply(
            [FromBody] ApplyCalibrationRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var results = await _calibrationService.ApplyCalibrationAsync(dto, cancellationToken);
            return Ok(results);
        }

        [HttpPost("applytoproject")]
        public async Task<ActionResult<int>> ApplyToProject(
            [FromQuery] Guid projectId,
            [FromQuery] Guid elementId,
            CancellationToken cancellationToken = default)
        {
            var count = await _calibrationService.ApplyCalibrationToProjectAsync(
                projectId, elementId, cancellationToken);

            return Ok(count); // فعلاً NotImplemented از سرویس پرتاب می‌شود
        }

    }
}
