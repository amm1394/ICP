namespace Shared.Icp.Constants
{
    /// <summary>
    /// Centralized validation constants used across the domain, data, and API layers.
    /// </summary>
    /// <remarks>
    /// These limits provide a single source of truth for length and range validations.
    /// Units are documented per constant (e.g., grams, milliliters, ppm). User-facing
    /// validation messages should remain localized in Persian in higher layers (e.g., DTO
    /// attributes, middleware, controllers). This class only defines numeric/string bounds.
    /// <para>
    /// Example usage (Data Annotations):
    /// [StringLength(ValidationConstants.SampleNameMaxLength)]
    /// public string Name { get; set; } = string.Empty;
    /// </para>
    /// <para>
    /// Example usage (FluentValidation):
    /// RuleFor(x => x.Weight).InclusiveBetween(ValidationConstants.MinWeight, ValidationConstants.MaxWeight);
    /// </para>
    /// </remarks>
    public static class ValidationConstants
    {
        // =====================
        // String Length Limits
        // =====================

        /// <summary>
        /// Maximum allowed length for a sample ID string.
        /// </summary>
        public const int SampleIdMaxLength = 100;

        /// <summary>
        /// Maximum allowed length for a sample name string.
        /// </summary>
        public const int SampleNameMaxLength = 200;

        /// <summary>
        /// Maximum allowed length for a chemical element symbol (e.g., "Fe").
        /// </summary>
        public const int ElementSymbolMaxLength = 10;

        /// <summary>
        /// Maximum allowed length for a chemical element name (e.g., "Iron").
        /// </summary>
        public const int ElementNameMaxLength = 100;

        /// <summary>
        /// Maximum allowed length for a Certified Reference Material (CRM) identifier.
        /// </summary>
        public const int CRMIdMaxLength = 50;

        /// <summary>
        /// Maximum allowed length for a Certified Reference Material (CRM) name.
        /// </summary>
        public const int CRMNameMaxLength = 200;

        /// <summary>
        /// Maximum allowed length for a project name.
        /// </summary>
        public const int ProjectNameMaxLength = 200;

        /// <summary>
        /// Maximum allowed length for generic notes or comments.
        /// </summary>
        public const int NotesMaxLength = 1000;

        // =====================
        // Numeric Range Limits
        // =====================

        /// <summary>
        /// Minimum allowed sample weight in grams (g).
        /// </summary>
        public const decimal MinWeight = 0.0001m; // گرم

        /// <summary>
        /// Maximum allowed sample weight in grams (g).
        /// </summary>
        public const decimal MaxWeight = 10000m; // گرم

        /// <summary>
        /// Minimum allowed sample volume in milliliters (mL).
        /// </summary>
        public const decimal MinVolume = 0.001m; // میلی‌لیتر

        /// <summary>
        /// Maximum allowed sample volume in milliliters (mL).
        /// </summary>
        public const decimal MaxVolume = 100000m; // میلی‌لیتر

        /// <summary>
        /// Minimum allowed dilution factor (dimensionless, must be &gt;= 1).
        /// </summary>
        public const decimal MinDilutionFactor = 1m;

        /// <summary>
        /// Maximum allowed dilution factor (dimensionless).
        /// </summary>
        public const decimal MaxDilutionFactor = 10000m;

        // =====================
        // Concentration Ranges
        // =====================

        /// <summary>
        /// Minimum allowed concentration (in parts per million, ppm). Use appropriate conversion when working in ppb.
        /// </summary>
        public const decimal MinConcentration = 0m;

        /// <summary>
        /// Maximum allowed concentration (in parts per million, ppm).
        /// </summary>
        public const decimal MaxConcentration = 1000000m; // ppm

        // =================
        // Intensity Ranges
        // =================

        /// <summary>
        /// Minimum allowed instrument signal intensity.
        /// </summary>
        public const decimal MinIntensity = 0m;

        /// <summary>
        /// Maximum allowed instrument signal intensity (instrument-specific upper bound).
        /// </summary>
        public const decimal MaxIntensity = 100000000m;

        // ======================
        // Atomic Number Ranges
        // ======================

        /// <summary>
        /// Minimum valid atomic number for chemical elements (Hydrogen = 1).
        /// </summary>
        public const int MinAtomicNumber = 1;

        /// <summary>
        /// Maximum valid atomic number for known elements (currently 118).
        /// </summary>
        public const int MaxAtomicNumber = 118;

        // ==============
        // R² (R-Squared)
        // ==============

        /// <summary>
        /// Minimum coefficient of determination value (R²), representing the lower bound of goodness of fit. Range is inclusive [0, 1].
        /// </summary>
        public const decimal MinRSquared = 0m;

        /// <summary>
        /// Maximum coefficient of determination value (R²). Values near 1 indicate strong linear fit. Range is inclusive [0, 1].
        /// </summary>
        public const decimal MaxRSquared = 1m;
    }
}