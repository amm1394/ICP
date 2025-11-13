namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// General/common error messages not tied to a specific domain area.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class General
        {
            /// <summary>
            /// Displayed for unexpected/unhandled errors.
            /// </summary>
            public const string UnexpectedError = "خطای غیرمنتظره رخ داده است";

            /// <summary>
            /// Displayed when a database connectivity or execution error occurs.
            /// </summary>
            public const string DatabaseError = "خطا در ارتباط با پایگاه داده";

            /// <summary>
            /// Displayed for generic data validation failures.
            /// </summary>
            public const string ValidationError = "خطا در اعتبارسنجی داده‌ها";

            /// <summary>
            /// Displayed when the user is not authorized to access the requested resource or action.
            /// </summary>
            public const string Unauthorized = "شما مجوز دسترسی به این بخش را ندارید";

            /// <summary>
            /// Displayed when the requested resource cannot be found.
            /// </summary>
            public const string NotFound = "موردی یافت نشد";

            /// <summary>
            /// Displayed when an identifier value is invalid (format or range).
            /// </summary>
            public const string InvalidId = "شناسه نامعتبر است";

            /// <summary>
            /// Displayed when a required field is missing.
            /// </summary>
            public const string RequiredField = "این فیلد الزامی است";

            /// <summary>
            /// Displayed when a value does not match the expected format.
            /// </summary>
            public const string InvalidFormat = "فرمت داده نامعتبر است";
        }
    }
}
