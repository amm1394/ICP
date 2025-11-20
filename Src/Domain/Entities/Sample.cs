using Core.Icp.Domain.Common;
using Core.Icp.Domain.ValueObjects;
using System.Diagnostics.Metrics;

namespace Core.Icp.Domain.Entities;

public class Sample : AuditableEntity
{
    public Guid Id { get; set; }
    public string SampleId { get; private set; } = null!;
    public string SampleName { get; private set; } = null!;
    public DateTime? RunDate { get; private set; }
    public double? Weight { get; private set; }           // گرم
    public double? Volume { get; private set; }           // میلی‌لیتر
    public DilutionFactor DilutionFactor { get; private set; } = DilutionFactor.Default;

    public ICollection<Measurement> Measurements { get; private set; } = new List<Measurement>();

    // برای QC
    public bool IsDeleted { get; private set; }
    public string? DeletionReason { get; private set; }

    // متدهای QC در Application می‌آیند، اینجا فقط propertyها
    public void MarkAsDeleted(string reason)
    {
        IsDeleted = true;
        DeletionReason = reason;
    }
}