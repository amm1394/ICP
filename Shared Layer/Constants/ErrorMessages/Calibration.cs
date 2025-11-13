namespace Shared.Icp.Constants
{
    /// <summary>
    /// Centralized Persian error messages used across the application.
    /// </summary>
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to calibration operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class Calibration
        {
            /// <summary>
            /// Displayed when the requested calibration cannot be found.
            /// </summary>
            public const string NotFound = "کالیبراسیون مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided calibration data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات کالیبراسیون نامعتبر است";

            /// <summary>
            /// Displayed when there are not enough calibration points to compute a curve.
            /// </summary>
            public const string InsufficientPoints = "تعداد نقاط کالیبراسیون کافی نیست";

            /// <summary>
            /// Displayed when the R-squared (coefficient of determination) value is invalid or below the acceptable threshold.
            /// </summary>
            public const string InvalidRSquared = "ضریب همبستگی نامعتبر است";

            /// <summary>
            /// Displayed when the calibration curve cannot be found.
            /// </summary>
            public const string CurveNotFound = "منحنی کالیبراسیون یافت نشد";
        }
    }
}
