namespace Shared.Icp.Exceptions
{
    /// <summary>
    /// Custom exception indicating a violation of a business rule.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inherits from <see cref="BaseException"/> and sets the error <see cref="BaseException.Code"/> to
    /// "BUSINESS_RULE_VIOLATION". Use this exception when domain constraints or invariants are not satisfied
    /// (e.g., invalid state transitions, disallowed operations, unmet preconditions).
    /// </para>
    /// <para>
    /// Localization: although documentation is in English, user-facing texts (exception messages shown to end users)
    /// should remain Persian for presentation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: disallow approving a project without required QC checks
    /// if (!project.HasRequiredQc)
    ///     throw new BusinessRuleException("امکان تایید پروژه بدون کنترل‌های کیفیت الزامی وجود ندارد");
    /// </code>
    /// </example>
    /// <seealso cref="ValidationException"/>
    /// <seealso cref="NotFoundException"/>
    /// <seealso cref="FileProcessingException"/>
    public class BusinessRuleException : BaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a custom message.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="BaseException.Code"/> to "BUSINESS_RULE_VIOLATION".
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for user-facing contexts).</param>
        public BusinessRuleException(string message)
            : base(message, "BUSINESS_RULE_VIOLATION")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a custom message and inner exception.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="BaseException.Code"/> to "BUSINESS_RULE_VIOLATION" and preserves the underlying exception
        /// in <see cref="System.Exception.InnerException"/> for diagnostics.
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for user-facing contexts).</param>
        /// <param name="innerException">The original exception that led to this rule violation.</param>
        public BusinessRuleException(string message, Exception innerException)
            : base(message, "BUSINESS_RULE_VIOLATION", innerException)
        {
        }
    }
}