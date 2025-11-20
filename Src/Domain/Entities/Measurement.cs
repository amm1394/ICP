namespace Core.Icp.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; }
    public Guid SampleId { get; set; }
    public int ElementId { get; set; }
    // یا Guid اگر بخوای
    public string ElementSymbol { get; private set; } = null!;

    public double? NetIntensity { get; private set; }
    public double? Concentration { get; private set; }

    // این خط رو تغییر بده → internal set
    public double? FinalConcentration { get; set; }

    public bool IsBlank { get; set; }
    public bool IsValid { get; set; } = true;
}