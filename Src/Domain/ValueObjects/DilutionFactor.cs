namespace Core.Icp.Domain.ValueObjects;

public record DilutionFactor(double Value)
{
    public static DilutionFactor Default => new(1.0);

    public static DilutionFactor FromDouble(double? value) => value.HasValue ? new DilutionFactor(value.Value) : Default;

    public double CalculateFinalConcentration(double raw) => raw * Value;
}