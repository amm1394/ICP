namespace Core.Icp.Domain.Entities;

public class Element
{
    public int Id { get; set; }
    public string Symbol { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int AtomicNumber { get; set; }
    public bool IsSelected { get; set; }
    public bool IsBlank { get; set; }  // برای BLK
    public bool IsInternalStandard { get; set; }
}