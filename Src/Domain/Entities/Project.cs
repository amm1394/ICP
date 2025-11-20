using Core.Icp.Domain.Common;
using System.Xml.Linq;

namespace Core.Icp.Domain.Entities;

public class Project : AuditableEntity
{
    public string Name { get; set; } = null!;
    public DateTime AnalysisDate { get; set; }

    public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    public ICollection<Element> SelectedElements { get; set; } = new List<Element>();

    // تنظیمات UI مثل DecimalPlaces, IntOxideMode و غیره
    public int DecimalPlaces { get; set; } = 3;
    public bool IsOxideMode { get; set; } = false;
}