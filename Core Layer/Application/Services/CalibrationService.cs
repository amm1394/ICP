using Core.Icp.Application.Interfaces;
using Core.Icp.Application.Models.Calibration;
using Core.Icp.Domain.Entities.Elements;
using Core.Icp.Domain.Interfaces.Repositories;

namespace Core.Icp.Application.Services
{
    public class CalibrationService : ICalibrationService
    {
        private readonly IRepository<CalibrationCurve> _curveRepository;
        private readonly IRepository<CalibrationPoint> _pointRepository;

        public CalibrationService(
            IRepository<CalibrationCurve> curveRepository,
            IRepository<CalibrationPoint> pointRepository)
        {
            _curveRepository = curveRepository;
            _pointRepository = pointRepository;
        }

        #region Create & Read

        public async Task<CalibrationCurveDto> CreateCurveAsync(
            CreateCalibrationCurveDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!string.Equals(dto.FitType, "Linear", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Only Linear fit is supported at this stage.");

            var fitPoints = dto.Points
                .Where(p => p.IsUsedInFit)
                .ToList();

            if (fitPoints.Count < 2)
                throw new InvalidOperationException("At least two points are required for linear regression.");

            var (slope, intercept, r2) = CalculateLinearRegression(fitPoints);

            var curve = new CalibrationCurve
            {
                Id = Guid.NewGuid(),
                ElementId = dto.ElementId,
                ProjectId = dto.ProjectId,
                Slope = slope,
                Intercept = intercept,
                RSquared = r2,
                FitType = dto.FitType,
                Degree = dto.Degree,
                IsActive = true,
                SettingsJson = dto.SettingsJson,
                CreatedAt = DateTime.UtcNow
            };

            curve = await _curveRepository.AddAsync(curve, cancellationToken);

            var points = dto.Points.Select(p => new CalibrationPoint
            {
                Id = Guid.NewGuid(),
                CalibrationCurveId = curve.Id,
                Concentration = p.Concentration,
                Intensity = p.Intensity,
                IsUsedInFit = p.IsUsedInFit,
                Order = p.Order,
                Label = p.Label,
                PointType = p.PointType
            }).ToList();

            await _pointRepository.AddRangeAsync(points, cancellationToken);

            curve.Points = points;

            return MapToDto(curve);
        }

        public async Task<CalibrationCurveDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var curve = await _curveRepository.GetByIdAsync(id, cancellationToken);
            if (curve == null)
                return null;

            var points = await _pointRepository.FindAsync(
                p => p.CalibrationCurveId == id,
                cancellationToken);

            curve.Points = points.ToList();

            return MapToDto(curve);
        }

        public async Task<IReadOnlyCollection<CalibrationCurveDto>> GetCurvesForProjectAsync(
            Guid projectId,
            Guid? elementId = null,
            bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var curves = await _curveRepository.FindAsync(c =>
                    c.ProjectId == projectId &&
                    (!elementId.HasValue || c.ElementId == elementId.Value),
                cancellationToken);

            if (onlyActive)
            {
                curves = curves.Where(c => c.IsActive).ToList();
            }

            var curveList = curves.ToList();
            var curveIds = curveList.Select(c => c.Id).ToList();

            var allPoints = await _pointRepository.FindAsync(
                p => curveIds.Contains(p.CalibrationCurveId),
                cancellationToken);

            var pointsByCurve = allPoints
                .GroupBy(p => p.CalibrationCurveId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var c in curveList)
            {
                if (pointsByCurve.TryGetValue(c.Id, out var curvePoints))
                    c.Points = curvePoints;
                else
                    c.Points = new List<CalibrationPoint>();
            }

            return curveList
                .Select(MapToDto)
                .ToList();
        }

        #endregion

        #region Status

        public async Task DeactivateCurveAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var curve = await _curveRepository.GetByIdAsync(id, cancellationToken);
            if (curve == null)
                return;

            curve.IsActive = false;
            await _curveRepository.UpdateAsync(curve, cancellationToken);
        }

        #endregion

        #region Evaluate

        public async Task<decimal?> EvaluateConcentrationAsync(
            Guid projectId,
            Guid elementId,
            decimal intensity,
            CancellationToken cancellationToken = default)
        {
            var curves = await _curveRepository.FindAsync(c =>
                    c.ProjectId == projectId &&
                    c.ElementId == elementId &&
                    c.IsActive,
                cancellationToken);

            var curve = curves
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            if (curve == null)
                return null;

            if (curve.Slope == 0)
                throw new InvalidOperationException("Calibration curve slope is zero.");

            var concentration = (intensity - curve.Intercept) / curve.Slope;
            return concentration;
        }

        /// <summary>
        /// این متد بعداً روی جدول نتایج واقعی پیاده‌سازی می‌شود.
        /// فعلاً برای سازگاری فاز ۴ فقط NotImplemented است.
        /// </summary>
        public Task<int> ApplyCalibrationToProjectAsync(
            Guid projectId,
            Guid elementId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(
                "ApplyCalibrationToProjectAsync will be implemented when measurement results entity is finalized.");
        }

        #endregion

        #region Helpers

        private static CalibrationCurveDto MapToDto(CalibrationCurve curve)
        {
            return new CalibrationCurveDto
            {
                Id = curve.Id,
                ElementId = curve.ElementId,
                ProjectId = curve.ProjectId,
                Slope = curve.Slope,
                Intercept = curve.Intercept,
                RSquared = curve.RSquared,
                FitType = curve.FitType,
                Degree = curve.Degree,
                IsActive = curve.IsActive,
                SettingsJson = curve.SettingsJson,
                Points = curve.Points?
                    .OrderBy(p => p.Order)
                    .Select(p => new CalibrationPointDto
                    {
                        Id = p.Id,
                        Concentration = p.Concentration,
                        Intensity = p.Intensity,
                        IsUsedInFit = p.IsUsedInFit,
                        Order = p.Order,
                        Label = p.Label,
                        PointType = p.PointType
                    })
                    .ToList()
                    ?? new List<CalibrationPointDto>()
            };
        }

        /// <summary>
        /// رگرسیون خطی ساده (Slope, Intercept, R²)
        /// </summary>
        private static (decimal slope, decimal intercept, decimal rSquared)
            CalculateLinearRegression(IList<CreateCalibrationPointDto> points)
        {
            var xs = points.Select(p => (double)p.Concentration).ToArray();
            var ys = points.Select(p => (double)p.Intensity).ToArray();

            int n = xs.Length;
            double meanX = xs.Average();
            double meanY = ys.Average();

            double sumXY = 0;
            double sumXX = 0;
            for (int i = 0; i < n; i++)
            {
                var dx = xs[i] - meanX;
                var dy = ys[i] - meanY;
                sumXY += dx * dy;
                sumXX += dx * dx;
            }

            if (sumXX == 0)
                throw new InvalidOperationException("Cannot compute slope: all X values are identical.");

            double slope = sumXY / sumXX;
            double intercept = meanY - slope * meanX;

            double ssTot = 0;
            double ssRes = 0;
            for (int i = 0; i < n; i++)
            {
                double yi = ys[i];
                double yiPred = slope * xs[i] + intercept;
                ssRes += Math.Pow(yi - yiPred, 2);
                ssTot += Math.Pow(yi - meanY, 2);
            }

            double r2 = ssTot == 0 ? 1.0 : 1 - (ssRes / ssTot);

            return ((decimal)slope, (decimal)intercept, (decimal)r2);
        }

        public async Task<CalibrationSummaryDto?> GetCalibrationSummaryAsync(
            Guid projectId,
            Guid elementId,
            CancellationToken cancellationToken = default)
        {
            // منحنی‌های فعال این پروژه/عنصر
            var curves = await _curveRepository.FindAsync(c =>
                    c.ProjectId == projectId &&
                    c.ElementId == elementId &&
                    c.IsActive,
                cancellationToken);

            var curve = curves
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            if (curve == null)
                return null;

            // تعداد نقاط این منحنی را هم می‌گیریم
            var points = await _pointRepository.FindAsync(
                p => p.CalibrationCurveId == curve.Id,
                cancellationToken);

            return new CalibrationSummaryDto
            {
                ProjectId = curve.ProjectId,
                ElementId = curve.ElementId,
                CurveId = curve.Id,
                Slope = curve.Slope,
                Intercept = curve.Intercept,
                RSquared = curve.RSquared,
                PointsCount = points.Count(),
                CreatedAt = curve.CreatedAt,
                IsActive = curve.IsActive,
                FitType = curve.FitType,
                Degree = curve.Degree
            };
        }

        public async Task<QcCheckResultDto?> EvaluateQcAsync(
            Guid projectId,
            Guid elementId,
            decimal expectedConcentration,
            decimal intensity,
            decimal tolerancePercent,
            CancellationToken cancellationToken = default)
        {
            // استفاده از همان متد EvaluateConcentrationAsync برای به‌دست آوردن غلظت محاسبه‌شده
            var measuredConcentration = await EvaluateConcentrationAsync(
                projectId,
                elementId,
                intensity,
                cancellationToken);

            if (measuredConcentration is null)
                return null; // یعنی منحنی فعالی برای این پروژه/عنصر وجود ندارد

            if (expectedConcentration == 0)
                throw new InvalidOperationException("Expected concentration cannot be zero.");

            var errorPercent =
                ((measuredConcentration.Value - expectedConcentration) / expectedConcentration) * 100m;

            var isPass = Math.Abs(errorPercent) <= tolerancePercent;

            return new QcCheckResultDto
            {
                ProjectId = projectId,
                ElementId = elementId,
                ExpectedConcentration = expectedConcentration,
                MeasuredConcentration = measuredConcentration.Value,
                Intensity = intensity,
                ErrorPercent = errorPercent,
                TolerancePercent = tolerancePercent,
                IsPass = isPass,
                Message = isPass
                    ? "QC passed within tolerance."
                    : "QC failed: error beyond tolerance."
            };
        }

        public async Task<IReadOnlyCollection<CalibratedSampleResultDto>> ApplyCalibrationAsync(
    ApplyCalibrationRequestDto dto,
    CancellationToken cancellationToken = default)
        {
            var projectId = dto.ProjectId;
            var elementId = dto.ElementId;

            // ۱. پیدا کردن منحنی فعال (مثل Evaluate)
            var curves = await _curveRepository.FindAsync(c =>
                    c.ProjectId == projectId &&
                    c.ElementId == elementId &&
                    c.IsActive,
                cancellationToken);

            var curve = curves
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            if (curve == null)
            {
                // در صورت نبود منحنی، برای همه نمونه‌ها خطا می‌گذاریم
                return dto.Samples
                    .Select(s => new CalibratedSampleResultDto
                    {
                        SampleKey = s.SampleKey,
                        Intensity = s.Intensity,
                        Error = "No active calibration curve found for this project/element."
                    })
                    .ToList();
            }

            if (curve.Slope == 0)
            {
                return dto.Samples
                    .Select(s => new CalibratedSampleResultDto
                    {
                        SampleKey = s.SampleKey,
                        Intensity = s.Intensity,
                        Error = "Calibration curve slope is zero."
                    })
                    .ToList();
            }

            // ۲. محاسبه غلظت برای هر نمونه
            var results = new List<CalibratedSampleResultDto>();

            foreach (var sample in dto.Samples)
            {
                var conc = (sample.Intensity - curve.Intercept) / curve.Slope;

                results.Add(new CalibratedSampleResultDto
                {
                    SampleKey = sample.SampleKey,
                    Intensity = sample.Intensity,
                    Concentration = conc,
                    Error = null
                });
            }

            return results;
        }
        #endregion
    }
}
