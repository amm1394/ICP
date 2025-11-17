using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Application.Models.Calibration;

namespace Core.Icp.Application.Interfaces
{
    public interface ICalibrationService
    {
        Task<CalibrationCurveDto> CreateCurveAsync(
            CreateCalibrationCurveDto dto,
            CancellationToken cancellationToken = default);

        Task<CalibrationCurveDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CalibrationCurveDto>> GetCurvesForProjectAsync(
            Guid projectId,
            Guid? elementId = null,
            bool onlyActive = true,
            CancellationToken cancellationToken = default);

        Task DeactivateCurveAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<decimal?> EvaluateConcentrationAsync(
            Guid projectId,
            Guid elementId,
            decimal intensity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// در آینده روی جدول نتایج واقعی پیاده‌سازی می‌شود.
        /// فعلاً فقط امضا نگه داشته شده است.
        /// </summary>
        Task<int> ApplyCalibrationToProjectAsync(
            Guid projectId,
            Guid elementId,
            CancellationToken cancellationToken = default);

        Task<CalibrationSummaryDto?> GetCalibrationSummaryAsync(
            Guid projectId,
            Guid elementId,
            CancellationToken cancellationToken = default);

        Task<QcCheckResultDto?> EvaluateQcAsync(
            Guid projectId,
            Guid elementId,
            decimal expectedConcentration,
            decimal intensity,
            decimal tolerancePercent,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CalibratedSampleResultDto>> ApplyCalibrationAsync(
            ApplyCalibrationRequestDto dto,
            CancellationToken cancellationToken = default);
    }
}
