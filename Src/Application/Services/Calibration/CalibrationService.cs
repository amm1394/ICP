using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using Shared.Helpers;

namespace Application.Services.Calibration;

public class CalibrationService(IUnitOfWork unitOfWork) : ICalibrationService
{
    public async Task<CalibrationCurve> CalculateAndSaveCurveAsync(Guid projectId, string elementName, CancellationToken cancellationToken = default)
    {
        // 1. دریافت تمام نمونه‌های استاندارد (Standard) پروژه
        var standards = await unitOfWork.Repository<Sample>()
            .GetAsync(s => s.ProjectId == projectId && s.Type == SampleType.Standard,
                      includeProperties: "Measurements");

        // 2. استخراج نقاط (غلظت و شدت) برای عنصر مورد نظر
        var points = new List<CalibrationPoint>();

        foreach (var std in standards)
        {
            // پیدا کردن اندازه‌گیری مربوط به این عنصر
            var measurement = std.Measurements.FirstOrDefault(m => m.ElementName == elementName);

            // فرض: در استانداردها، فیلد Weight یا یک فیلد خاص نشان‌دهنده غلظت استاندارد است.
            // در اینجا فرض می‌کنیم کاربر غلظت استاندارد را در فیلد Weight وارد کرده (طبق منطق فایل‌های قدیمی)
            // یا اینکه باید سیستمی برای ورود غلظت استاندارد داشته باشیم.
            // فعلاً از Weight به عنوان غلظت استاندارد (X) استفاده می‌کنیم.

            if (measurement != null)
            {
                points.Add(new CalibrationPoint
                {
                    Concentration = std.Weight, // محور X: غلظت معلوم استاندارد
                    Intensity = measurement.Value, // محور Y: شدت خوانده شده دستگاه
                    IsExcluded = false
                });
            }
        }

        // 3. محاسبه رگرسیون
        var xValues = points.Select(p => p.Concentration).ToList();
        var yValues = points.Select(p => p.Intensity).ToList();

        var (slope, intercept, rSquared) = MathHelper.CalculateLinearRegression(xValues, yValues);

        // 4. ساخت و ذخیره منحنی
        var curve = new CalibrationCurve
        {
            ProjectId = projectId,
            ElementName = elementName,
            Slope = slope,
            Intercept = intercept,
            RSquared = rSquared,
            IsActive = true,
            Points = points
        };

        // غیرفعال کردن منحنی‌های قبلی این عنصر
        var oldCurves = await unitOfWork.Repository<CalibrationCurve>()
            .GetAsync(c => c.ProjectId == projectId && c.ElementName == elementName && c.IsActive);
        foreach (var old in oldCurves) old.IsActive = false;

        await unitOfWork.Repository<CalibrationCurve>().AddAsync(curve);
        await unitOfWork.CommitAsync(cancellationToken);

        return curve;
    }

    public double CalculateConcentration(double intensity, CalibrationCurve curve)
    {
        if (curve.Slope == 0) return 0;
        // فرمول: Concentration = (Intensity - Intercept) / Slope
        return (intensity - curve.Intercept) / curve.Slope;
    }
}