namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to file operations (uploading, reading, writing, etc.).
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class File
        {
            /// <summary>
            /// Displayed when the file cannot be found.
            /// </summary>
            public const string NotFound = "فایل مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the file format is invalid or not recognized.
            /// </summary>
            public const string InvalidFormat = "فرمت فایل نامعتبر است";

            /// <summary>
            /// Displayed when the uploaded or provided file is empty.
            /// </summary>
            public const string EmptyFile = "فایل خالی است";

            /// <summary>
            /// Displayed when the file exceeds the maximum allowed size.
            /// </summary>
            public const string TooLarge = "حجم فایل بیش از حد مجاز است";

            /// <summary>
            /// Displayed when an error occurs while reading the file from storage or stream.
            /// </summary>
            public const string ReadError = "خطا در خواندن فایل";

            /// <summary>
            /// Displayed when an error occurs while writing the file to storage or stream.
            /// </summary>
            public const string WriteError = "خطا در نوشتن فایل";

            /// <summary>
            /// Displayed when the file format is not supported by the application.
            /// </summary>
            public const string UnsupportedFormat = "فرمت فایل پشتیبانی نمی‌شود";
        }
    }
}
