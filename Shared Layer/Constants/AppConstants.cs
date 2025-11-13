namespace Shared.Icp.Constants
{
    /// <summary>
    /// Centralized application-wide constants.
    /// </summary>
    /// <remarks>
    /// This class aggregates immutable values used across the solution: application metadata, units,
    /// file extensions, pagination defaults, quality check thresholds, calibration limits, date/time
    /// formats, and string labels for statuses and check types. User-facing messages remain localized
    /// in Persian elsewhere (e.g., controllers, middleware); the string labels defined here are
    /// intended as stable codes/identifiers and may be displayed with localized equivalents in the UI.
    /// </remarks>
    public static class AppConstants
    {
        // ==================
        // Application Info
        // ==================

        /// <summary>
        /// The display name of the application.
        /// </summary>
        public const string ApplicationName = "Isatis.NET";

        /// <summary>
        /// The semantic version of the application.
        /// </summary>
        public const string ApplicationVersion = "1.0.0";

        // ======
        // Units
        // ======

        /// <summary>
        /// Concentration unit: parts per million.
        /// </summary>
        public const string UnitPpm = "ppm";

        /// <summary>
        /// Concentration unit: parts per billion.
        /// </summary>
        public const string UnitPpb = "ppb";

        /// <summary>
        /// Percentage sign used for tolerance/relative values.
        /// </summary>
        public const string UnitPercent = "%";

        /// <summary>
        /// Mass unit: grams.
        /// </summary>
        public const string UnitGram = "g";

        /// <summary>
        /// Volume unit: milliliters.
        /// </summary>
        public const string UnitMilliliter = "mL";

        // ==============
        // File Formats
        // ==============

        /// <summary>
        /// File extension for CSV files.
        /// </summary>
        public const string CsvExtension = ".csv";

        /// <summary>
        /// File extension for modern Excel workbooks.
        /// </summary>
        public const string ExcelExtension = ".xlsx";

        /// <summary>
        /// File extension for legacy Excel workbooks.
        /// </summary>
        public const string ExcelLegacyExtension = ".xls";

        /// <summary>
        /// File extension for PDF documents.
        /// </summary>
        public const string PdfExtension = ".pdf";

        /// <summary>
        /// File extension for serialized project files used by this application.
        /// </summary>
        public const string ProjectExtension = ".icp";

        // ===========
        // Pagination
        // ===========

        /// <summary>
        /// Default number of items to return per page when pagination is applied.
        /// </summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// Maximum number of items allowed per page to prevent excessive payloads.
        /// </summary>
        public const int MaxPageSize = 100;

        // ============================
        // Quality Check Thresholds (%) 
        // ============================

        /// <summary>
        /// Default acceptable deviation for weight checks (percent).
        /// </summary>
        public const decimal DefaultWeightTolerance = 5.0m; // درصد

        /// <summary>
        /// Default acceptable deviation for volume checks (percent).
        /// </summary>
        public const decimal DefaultVolumeTolerance = 5.0m; // درصد

        /// <summary>
        /// Default acceptable deviation for dilution factor checks (percent).
        /// </summary>
        public const decimal DefaultDFTolerance = 10.0m; // درصد

        /// <summary>
        /// Default acceptable deviation for CRM agreement checks (percent).
        /// </summary>
        public const decimal DefaultCRMTolerance = 10.0m; // درصد

        // ============
        // Calibration
        // ============

        /// <summary>
        /// Minimum acceptable coefficient of determination (R²) for a valid calibration curve.
        /// </summary>
        public const decimal MinAcceptableRSquared = 0.995m;

        /// <summary>
        /// Minimum number of calibration points required to compute a curve.
        /// </summary>
        public const int MinCalibrationPoints = 3;

        /// <summary>
        /// Maximum number of calibration points supported for a curve.
        /// </summary>
        public const int MaxCalibrationPoints = 10;

        // =============
        // Date Formats
        // =============

        /// <summary>
        /// Standard date format string (UTC/local context determined by usage).
        /// </summary>
        public const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Standard date-time format string (24-hour clock).
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Standard time-only format string (24-hour clock).
        /// </summary>
        public const string TimeFormat = "HH:mm:ss";

        // ==============
        // Sample Status
        // ==============

        /// <summary>
        /// Label for a sample that is awaiting processing. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string StatusPending = "Pending";

        /// <summary>
        /// Label for a sample that is currently being processed. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string StatusProcessing = "Processing";

        /// <summary>
        /// Label for a sample that has been processed. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string StatusProcessed = "Processed";

        /// <summary>
        /// Label for a sample that has been approved. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string StatusApproved = "Approved";

        /// <summary>
        /// Label for a sample that has been rejected. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string StatusRejected = "Rejected";

        /// <summary>
        /// Label for a sample requiring additional review. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string StatusRequiresReview = "RequiresReview";

        // ============
        // Check Types
        // ============

        /// <summary>
        /// Label for weight check type. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckTypeWeight = "WeightCheck";

        /// <summary>
        /// Label for volume check type. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckTypeVolume = "VolumeCheck";

        /// <summary>
        /// Label for dilution factor check type. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckTypeDF = "DilutionFactorCheck";

        /// <summary>
        /// Label for empty/blank check type. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckTypeEmpty = "EmptyCheck";

        /// <summary>
        /// Label for CRM agreement check type. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckTypeCRM = "CRMCheck";

        /// <summary>
        /// Label for instrument drift calibration check type. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckTypeDrift = "DriftCalibration";

        // ============
        // Check Status
        // ============

        /// <summary>
        /// Label for a passing check result. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckStatusPass = "Pass";

        /// <summary>
        /// Label for a failing check result. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckStatusFail = "Fail";

        /// <summary>
        /// Label for a warning check result. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckStatusWarning = "Warning";

        /// <summary>
        /// Label for a pending check result. Intended as a stable code; localize for display as needed.
        /// </summary>
        public const string CheckStatusPending = "Pending";
    }
}