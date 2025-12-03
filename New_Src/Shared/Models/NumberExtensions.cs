namespace Shared.Models;

/// <summary>
/// Extension methods for number formatting
/// Python-compatible: removes trailing zeros like Python's rstrip('0').rstrip('.')
/// 
/// Python equivalent (result.py format_value):
///   formatted = f"{value:.{decimal_places}f}".rstrip('0').rstrip('.')
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    /// Format decimal with specified decimal places, removing trailing zeros
    /// Matches Python behavior: f"{value:.2f}".rstrip('0').rstrip('.')
    /// 
    /// Examples:
    ///   1.50 → "1.5"
    ///   1.00 → "1"
    ///   1.23 → "1.23"
    ///   0.10 → "0.1"
    /// </summary>
    public static string FormatPython(this decimal value, int decimalPlaces = 2)
    {
        // Format with specified decimal places
        var formatted = value.ToString($"F{decimalPlaces}");
        
        // Remove trailing zeros and trailing decimal point (like Python rstrip)
        formatted = formatted.TrimEnd('0').TrimEnd('.');
        
        // Ensure at least "0" for zero values
        return string.IsNullOrEmpty(formatted) ? "0" : formatted;
    }

    /// <summary>
    /// Format nullable decimal with specified decimal places, removing trailing zeros
    /// </summary>
    public static string? FormatPython(this decimal? value, int decimalPlaces = 2)
    {
        return value?.FormatPython(decimalPlaces);
    }

    /// <summary>
    /// Format double with specified decimal places, removing trailing zeros
    /// </summary>
    public static string FormatPython(this double value, int decimalPlaces = 2)
    {
        return ((decimal)value).FormatPython(decimalPlaces);
    }

    /// <summary>
    /// Format nullable double with specified decimal places, removing trailing zeros
    /// </summary>
    public static string? FormatPython(this double? value, int decimalPlaces = 2)
    {
        return value?.FormatPython(decimalPlaces);
    }
}
