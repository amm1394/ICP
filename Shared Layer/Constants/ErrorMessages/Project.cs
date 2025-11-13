namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to Project operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class Project
        {
            /// <summary>
            /// Displayed when the requested project cannot be found.
            /// </summary>
            public const string NotFound = "پروژه مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided project data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات پروژه نامعتبر است";

            /// <summary>
            /// Displayed when a project name is missing but required.
            /// </summary>
            public const string NameRequired = "نام پروژه الزامی است";

            /// <summary>
            /// Displayed when the project name is already in use.
            /// </summary>
            public const string DuplicateName = "نام پروژه تکراری است";

            /// <summary>
            /// Displayed when a project cannot be deleted because it has associated samples.
            /// </summary>
            public const string HasSamples = "این پروژه دارای نمونه است و قابل حذف نیست";
        }
    }
}
