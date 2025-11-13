namespace Shared.Icp.Exceptions
{
    /// <summary>
    /// Custom exception representing file processing errors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inherits from <see cref="BaseException"/> and uses the error code "FILE_PROCESSING_ERROR" for all instances.
    /// It optionally carries the source file name and line number where the error occurred to aid diagnostics.
    /// </para>
    /// <para>
    /// Localization: although documentation is in English, user-facing texts (messages shown to end users) should
    /// remain Persian in presentation layers. Some constructors compose a Persian message that includes file and line info.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: simple message
    /// throw new FileProcessingException("پرونده ورودی قابل خواندن نیست");
    ///
    /// // Example: message with file name
    /// throw new FileProcessingException("قالب فایل نامعتبر است", "import.xlsx");
    ///
    /// // Example: message with file name and line number
    /// throw new FileProcessingException("ساختار ردیف نامعتبر است", "import.csv", 42);
    ///
    /// // Example: with inner exception
    /// try
    /// {
    ///     // parse file
    /// }
    /// catch (Exception ex)
    /// {
    ///     throw new FileProcessingException("بازیابی اطلاعات از فایل با خطا مواجه شد", "data.json", 7, ex);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="BaseException"/>
    /// <seealso cref="ValidationException"/>
    /// <seealso cref="NotFoundException"/>
    public class FileProcessingException : BaseException
    {
        /// <summary>
        /// Gets the name of the file being processed when the error occurred, if available.
        /// </summary>
        /// <remarks>
        /// Optional metadata for diagnostics; may be null when not provided.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        public string? FileName { get; }

        /// <summary>
        /// Gets the line number in the file associated with the error, if available.
        /// </summary>
        /// <remarks>
        /// Optional metadata for diagnostics; may be null when not provided. Line numbers are 1-based by convention.
        /// </remarks>
        /// <value>
        /// Integer value or null.
        /// </value>
        public int? LineNumber { get; }

        // Constructor 1: فقط پیام
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingException"/> class with a custom message.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="BaseException.Code"/> to "FILE_PROCESSING_ERROR".
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for user-facing contexts).</param>
        public FileProcessingException(string message)
            : base(message, "FILE_PROCESSING_ERROR")
        {
        }

        // Constructor 2: پیام + fileName
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingException"/> class with a message and file name.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="BaseException.Code"/> to "FILE_PROCESSING_ERROR" and stores <see cref="FileName"/>.
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for user-facing contexts).</param>
        /// <param name="fileName">The file name associated with the processing error.</param>
        public FileProcessingException(string message, string fileName)
            : base(message, "FILE_PROCESSING_ERROR")
        {
            FileName = fileName;
        }

        // Constructor 3: پیام + fileName + lineNumber
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingException"/> class with a message, file name, and line number.
        /// </summary>
        /// <remarks>
        /// Composes a Persian message that includes the file and line number, and sets <see cref="BaseException.Code"/> to
        /// "FILE_PROCESSING_ERROR". Also stores <see cref="FileName"/> and <see cref="LineNumber"/>.
        /// </remarks>
        /// <param name="message">Base error message (Persian recommended for presentation).</param>
        /// <param name="fileName">The file name associated with the error.</param>
        /// <param name="lineNumber">The line number in the file where the error occurred.</param>
        public FileProcessingException(string message, string fileName, int lineNumber)
            : base($"{message} (فایل: {fileName}, خط: {lineNumber})", "FILE_PROCESSING_ERROR")
        {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        // Constructor 4: پیام + innerException
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingException"/> class with a message and inner exception.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="BaseException.Code"/> to "FILE_PROCESSING_ERROR" and preserves the underlying exception in
        /// <see cref="System.Exception.InnerException"/> for diagnostics.
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for user-facing contexts).</param>
        /// <param name="innerException">The original exception that caused this error.</param>
        public FileProcessingException(string message, Exception innerException)
            : base(message, "FILE_PROCESSING_ERROR", innerException)
        {
        }

        // Constructor 5: پیام + fileName + innerException ← این مورد نیازه!
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingException"/> class with a message, file name, and inner exception.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="BaseException.Code"/> to "FILE_PROCESSING_ERROR" and stores <see cref="FileName"/>.
        /// The original exception is attached for diagnostic purposes.
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for presentation).</param>
        /// <param name="fileName">The file name associated with the error.</param>
        /// <param name="innerException">The original exception that triggered this error.</param>
        public FileProcessingException(string message, string fileName, Exception innerException)
            : base(message, "FILE_PROCESSING_ERROR", innerException)
        {
            FileName = fileName;
        }

        // Constructor 6: همه پارامترها (اختیاری - برای کامل بودن)
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingException"/> class with a message, file name, line number, and inner exception.
        /// </summary>
        /// <remarks>
        /// Composes a Persian message including file and line number, sets <see cref="BaseException.Code"/> to "FILE_PROCESSING_ERROR",
        /// and stores <see cref="FileName"/> and <see cref="LineNumber"/>. The underlying exception is attached for diagnostics.
        /// </remarks>
        /// <param name="message">Base error message (Persian recommended for presentation).</param>
        /// <param name="fileName">The file name associated with the error.</param>
        /// <param name="lineNumber">The line number in the file where the error occurred.</param>
        /// <param name="innerException">The original exception that caused this error.</param>
        public FileProcessingException(string message, string fileName, int lineNumber, Exception innerException)
            : base($"{message} (فایل: {fileName}, خط: {lineNumber})", "FILE_PROCESSING_ERROR", innerException)
        {
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}