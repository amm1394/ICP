using Domain.Entities;

namespace Domain.Interfaces.Services;

public interface ICalibrationService
{
    // محاسبه و ساخت منحنی برای یک عنصر در یک پروژه
    Task<CalibrationCurve> CalculateAndSaveCurveAsync(Guid projectId, string elementName, CancellationToken cancellationToken = default);

    // محاسبه غلظت یک نمونه مجهول با استفاده از منحنی موجود
    double CalculateConcentration(double intensity, CalibrationCurve curve);
}