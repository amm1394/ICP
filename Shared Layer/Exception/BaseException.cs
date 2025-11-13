namespace Shared.Icp.Exceptions  // ← از BaseException به Exceptions تغییر داد
{
    /// <summary>
    /// Abstract base class for all custom exceptions in the system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides a unified structure for error handling by carrying a machine-readable <see cref="Code"/>
    /// and a UTC <see cref="Timestamp"/> recorded at the time the exception instance is created.
    /// Derive domain-specific exceptions from this type to standardize error propagation across layers.
    /// </para>
    /// <para>
    /// Localization: while this documentation is in English, any user-facing messages should remain Persian in
    /// presentation layers. The <c>message</c> passed to constructors can be Persian for end-user display.
    /// </para>
    /// <para>
    /// Usage guidelines:
    /// - <see cref="Code"/> should be a stable, machine-readable token (e.g., NOT_FOUND, VALIDATION_ERROR).
    /// - Prefer throwing specialized derived exceptions (e.g., <see cref="NotFoundException"/>, <see cref="ValidationException"/>)
    ///   to improve handling and logging.
    /// - <see cref="Timestamp"/> is captured at construction and is not adjusted for time zone. Treat it as an audit/diagnostic marker.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example of a derived exception
    /// public sealed class ProjectAccessDeniedException : BaseException
    /// {
    ///     public ProjectAccessDeniedException(string message)
    ///         : base(message, "ACCESS_DENIED")
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="NotFoundException"/>
    /// <seealso cref="ValidationException"/>
    /// <seealso cref="BusinessRuleException"/>
    /// <seealso cref="FileProcessingException"/>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// Gets the machine-readable error code used to classify the error type.
        /// </summary>
        /// <remarks>
        /// Intended for programmatic handling and mapping to appropriate HTTP status codes or UI behaviors.
        /// Typical values are constant strings (e.g., "NOT_FOUND", "VALIDATION_ERROR"). The value is set once during construction
        /// and cannot be changed afterward.
        /// </remarks>
        /// <value>
        /// A non-empty string representing the error category.
        /// </value>
        public string Code { get; }

        /// <summary>
        /// Gets the timestamp (UTC) when the exception was instantiated.
        /// </summary>
        /// <remarks>
        /// Recorded using <see cref="System.DateTime.UtcNow"/> at construction time. Use for diagnostics and logging. The timestamp
        /// is immutable and does not reflect subsequent time zone conversions.
        /// </remarks>
        /// <value>
        /// A <see cref="System.DateTime"/> value in UTC.
        /// </value>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseException"/> class.
        /// </summary>
        /// <remarks>
        /// Sets the <see cref="Code"/> and captures the <see cref="Timestamp"/> in UTC.
        /// </remarks>
        /// <param name="message">Human-readable error message (Persian recommended for user-facing contexts).</param>
        /// <param name="code">Machine-readable error code used for classification.</param>
        protected BaseException(string message, string code)
            : base(message)
        {
            Code = code;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseException"/> class with an inner exception.
        /// </summary>
        /// <remarks>
        /// Sets the <see cref="Code"/>, captures the <see cref="Timestamp"/> in UTC, and preserves the underlying
        /// exception in <see cref="System.Exception.InnerException"/> for diagnostics.
        /// </remarks>
        /// <param name="message">Human-readable error message (Persian recommended for user-facing contexts).</param>
        /// <param name="code">Machine-readable error code used for classification.</param>
        /// <param name="innerException">The original exception that caused the current error.</param>
        protected BaseException(string message, string code, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
            Timestamp = DateTime.UtcNow;
        }
    }
}