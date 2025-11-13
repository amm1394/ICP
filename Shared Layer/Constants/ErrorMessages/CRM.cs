namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to CRM (Certified Reference Material) operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class CRM
        {
            /// <summary>
            /// Displayed when the requested CRM cannot be found.
            /// </summary>
            public const string NotFound = "ماده مرجع مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided CRM data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات ماده مرجع نامعتبر است";

            /// <summary>
            /// Displayed when a CRM ID is missing but required.
            /// </summary>
            public const string CRMIdRequired = "شناسه ماده مرجع الزامی است";

            /// <summary>
            /// Displayed when a CRM name is missing but required.
            /// </summary>
            public const string NameRequired = "نام ماده مرجع الزامی است";

            /// <summary>
            /// Displayed when the CRM ID is already in use.
            /// </summary>
            public const string DuplicateCRMId = "شناسه ماده مرجع تکراری است";

            /// <summary>
            /// Displayed when a certified value is missing but required.
            /// </summary>
            public const string ValueRequired = "مقدار گواهی شده الزامی است";

            /// <summary>
            /// Displayed when the certified value provided is less than or equal to zero.
            /// </summary>
            public const string ValueInvalid = "مقدار گواهی شده باید بزرگتر از صفر باشد";
        }
    }
}
