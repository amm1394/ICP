namespace Shared.Icp.Constants
{
    public static partial class ErrorMessages
    {
        /// <summary>
        /// Error messages related to Element operations and validations.
        /// </summary>
        /// <remarks>
        /// Messages are user-facing and intentionally localized in Persian. Use these constants to ensure
        /// consistent error text across services, validators, and API responses.
        /// </remarks>
        public static class Element
        {
            /// <summary>
            /// Displayed when the requested element cannot be found.
            /// </summary>
            public const string NotFound = "عنصر مورد نظر یافت نشد";

            /// <summary>
            /// Displayed when the provided element data is invalid.
            /// </summary>
            public const string InvalidData = "اطلاعات عنصر نامعتبر است";

            /// <summary>
            /// Displayed when an element symbol is missing but required.
            /// </summary>
            public const string SymbolRequired = "نماد عنصر الزامی است";

            /// <summary>
            /// Displayed when an element name is missing but required.
            /// </summary>
            public const string NameRequired = "نام عنصر الزامی است";

            /// <summary>
            /// Displayed when the element symbol is already in use.
            /// </summary>
            public const string DuplicateSymbol = "نماد عنصر تکراری است";

            /// <summary>
            /// Displayed when the atomic number is already in use by another element.
            /// </summary>
            public const string DuplicateAtomicNumber = "عدد اتمی تکراری است";
        }
    }
}
