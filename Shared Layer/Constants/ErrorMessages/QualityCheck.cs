namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to Quality Control operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class QualityCheck
        {
            /// <summary>
            /// Displayed when the requested quality check cannot be found.
            /// </summary>
            public const string NotFound = "کنترل کیفیت مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided quality control data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات کنترل کیفیت نامعتبر است";

            /// <summary>
            /// Displayed when a quality control check fails.
            /// </summary>
            public const string CheckFailed = "کنترل کیفیت ناموفق بود";
        }
    }
}
