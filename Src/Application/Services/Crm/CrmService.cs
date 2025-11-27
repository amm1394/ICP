using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using MathNet.Numerics.LinearRegression;

namespace Application.Services.Crm; // ✅ اصلاح Namespace به Crm (حروف کوچک)

public class CrmService(IUnitOfWork unitOfWork) : ICrmService
{
    public async Task<Dictionary<string, double>> GetCertifiedValuesAsync(string crmName, CancellationToken cancellationToken = default)
    {
        var crms = await unitOfWork.Repository<Domain.Entities.Crm>()
            .GetAsync(c => c.Name == crmName, includeProperties: "CertifiedValues");

        var crm = crms.FirstOrDefault();

        if (crm == null || crm.CertifiedValues == null)
        {
            return new Dictionary<string, double>();
        }

        // ✅ اصلاح نام پراپرتی‌ها طبق CrmCertifiedValue.cs
        return crm.CertifiedValues
            .ToDictionary(
                v => v.ElementName, // قبلاً ElementSymbol بود
                v => v.Value        // قبلاً CertifiedValue بود
            );
    }

    public async Task<(double Blank, double Scale)> CalculateCorrectionFactorsAsync(
        Guid projectId,
        string elementName,
        CancellationToken cancellationToken = default)
    {
        var projectSamples = await unitOfWork.Repository<Sample>()
            .GetAsync(s => s.ProjectId == projectId && s.Type == SampleType.Standard,
                      includeProperties: "Measurements");

        var crmDataPoints = new List<(double Certified, double Measured)>();

        var allCrms = await unitOfWork.Repository<Domain.Entities.Crm>()
            .GetAsync(c => true, includeProperties: "CertifiedValues");

        foreach (var sample in projectSamples)
        {
            var matchedCrm = allCrms.FirstOrDefault(c =>
                sample.SolutionLabel.Contains(c.Name, StringComparison.OrdinalIgnoreCase));

            if (matchedCrm == null || matchedCrm.CertifiedValues == null) continue;

            // ✅ اصلاح نام پراپرتی‌ها
            var certifiedVal = matchedCrm.CertifiedValues
                .FirstOrDefault(v => v.ElementName.Equals(elementName, StringComparison.OrdinalIgnoreCase))?
                .Value;

            var measuredVal = sample.Measurements
                .FirstOrDefault(m => m.ElementName.Equals(elementName, StringComparison.OrdinalIgnoreCase))?
                .Value;

            if (certifiedVal.HasValue && measuredVal.HasValue && certifiedVal.Value > 0)
            {
                crmDataPoints.Add((certifiedVal.Value, measuredVal.Value));
            }
        }

        if (crmDataPoints.Count < 2)
        {
            return (0.0, 1.0);
        }

        var xData = crmDataPoints.Select(p => p.Certified).ToArray();
        var yData = crmDataPoints.Select(p => p.Measured).ToArray();

        var p = SimpleRegression.Fit(xData, yData);

        double calculatedBlank = p.Item1;
        double calculatedScale = p.Item2;

        if (calculatedScale <= 0.1 || calculatedScale > 10)
        {
            calculatedScale = 1.0;
            calculatedBlank = 0.0;
        }

        return (calculatedBlank, calculatedScale);
    }
}