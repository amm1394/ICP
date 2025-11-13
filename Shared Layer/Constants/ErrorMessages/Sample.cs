namespace Shared.Icp.Constants
{
    /// <summary>
    /// Centralized Persian error messages used across the application.
    /// </summary>
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to Sample operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class Sample
        {
            /// <summary>
            /// Displayed when the requested sample cannot be found.
            /// </summary>
            public const string NotFound = "نمونه مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided sample data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات نمونه نامعتبر است";

            /// <summary>
            /// Displayed when a sample ID is missing but required.
            /// </summary>
            public const string SampleIdRequired = "شناسه نمونه الزامی است";

            /// <summary>
            /// Displayed when a sample name is missing but required.
            /// </summary>
            public const string SampleNameRequired = "نام نمونه الزامی است";

            /// <summary>
            /// Displayed when the sample weight is less than or equal to zero.
            /// </summary>
            public const string WeightInvalid = "وزن نمونه باید بزرگتر از صفر باشد";

            /// <summary>
            /// Displayed when the sample volume is less than or equal to zero.
            /// </summary>
            public const string VolumeInvalid = "حجم نمونه باید بزرگتر از صفر باشد";

            /// <summary>
            /// Displayed when the dilution factor is less than or equal to zero.
            /// </summary>
            public const string DilutionFactorInvalid = "فاکتور رقیق‌سازی باید بزرگتر از صفر باشد";

            /// <summary>
            /// Displayed when the provided sample ID is already in use.
            /// </summary>
            public const string DuplicateSampleId = "شناسه نمونه تکراری است";

            /// <summary>
            /// Displayed when the sample cannot be deleted due to business constraints.
            /// </summary>
            public const string CannotDelete = "امکان حذف این نمونه وجود ندارد";

            /// <summary>
            /// Displayed when the sample cannot be edited due to business constraints.
            /// </summary>
            public const string CannotEdit = "امکان ویرایش این نمونه وجود ندارد";
        }
    }
}
