namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to Measurement operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class Measurement
        {
            /// <summary>
            /// Displayed when the requested measurement cannot be found.
            /// </summary>
            public const string NotFound = "اندازه‌گیری مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided measurement data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات اندازه‌گیری نامعتبر است";

            /// <summary>
            /// Displayed when a sample reference is missing but required.
            /// </summary>
            public const string SampleRequired = "نمونه الزامی است";

            /// <summary>
            /// Displayed when an element reference is missing but required.
            /// </summary>
            public const string ElementRequired = "عنصر الزامی است";

            /// <summary>
            /// Displayed when the signal intensity is less than or equal to zero.
            /// </summary>
            public const string IntensityInvalid = "شدت باید بزرگتر از صفر باشد";

            /// <summary>
            /// Displayed when the concentration value is negative.
            /// </summary>
            public const string ConcentrationInvalid = "غلظت نمی‌تواند منفی باشد";
        }
    }
}
