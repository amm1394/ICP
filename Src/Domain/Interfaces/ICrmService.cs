namespace Domain.Interfaces;

public interface ICrmService
{
    /// <summary>
    /// دریافت مقادیر تایید شده (Certified Values) برای یک ماده مرجع خاص
    /// خروجی: دیکشنری که کلید آن نام عنصر و مقدار آن غلظت تایید شده است.
    /// </summary>
    Task<Dictionary<string, double>> GetCertifiedValuesAsync(
        string crmName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// محاسبه فاکتورهای اصلاح (Blank, Scale) برای یک عنصر خاص در یک پروژه
    /// </summary>
    Task<(double Blank, double Scale)> CalculateCorrectionFactorsAsync(
        Guid projectId,
        string elementName,
        CancellationToken cancellationToken = default);
}