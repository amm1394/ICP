namespace Shared.Icp.Exceptions
{
    /// <summary>
    /// Custom exception indicating that a requested resource/entity was not found.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inherits from <see cref="BaseException"/> and sets the error <see cref="BaseException.Code"/> to "NOT_FOUND".
    /// Use this exception when a lookup by identifier or unique key fails to locate the target resource.
    /// </para>
    /// <para>
    /// Localization: although documentation is in English, user-facing texts (e.g., exception messages shown to end users)
    /// should remain Persian for presentation. The constructor overload that accepts entity name and key uses a Persian template.
    /// </para>
    /// <para>
    /// Usage guidelines:
    /// - Map this exception to HTTP 404 in web APIs.
    /// - Keep <c>message</c> content Persian for user-facing responses.
    /// - Prefer the (entityName, key) overload when available to produce a consistent Persian message.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: not found by numeric id
    /// if (project is null)
    ///     throw new NotFoundException("پروژه", 123);
    ///
    /// // Example: custom message (Persian recommended)
    /// throw new NotFoundException("نمونه مورد نظر یافت نشد");
    /// </code>
    /// </example>
    /// <seealso cref="BaseException"/>
    /// <seealso cref="ValidationException"/>
    /// <seealso cref="BusinessRuleException"/>
    /// <seealso cref="FileProcessingException"/>
    public class NotFoundException : BaseException
    {
        /// <summary>
        /// Creates a new <see cref="NotFoundException"/> with a custom message.
        /// </summary>
        /// <remarks>
        /// The <see cref="BaseException.Code"/> is set to "NOT_FOUND". Consider providing a Persian message for user-facing contexts.
        /// </remarks>
        /// <param name="message">Custom error message (Persian recommended for presentation).</param>
        public NotFoundException(string message)
            : base(message, "NOT_FOUND")
        {
        }

        /// <summary>
        /// Creates a new <see cref="NotFoundException"/> using a Persian template with the entity name and key.
        /// </summary>
        /// <remarks>
        /// Sets the <see cref="BaseException.Code"/> to "NOT_FOUND". The message is generated as a Persian sentence
        /// to be suitable for end-user presentation.
        /// </remarks>
        /// <param name="entityName">Display name of the entity (e.g., "پروژه", "نمونه"). Persian is recommended for UI display.</param>
        /// <param name="key">The identifier or key used in the lookup (e.g., numeric id, GUID, or code). It is interpolated into the Persian message.</param>
        public NotFoundException(string entityName, object key)
            : base($"{entityName} با شناسه {key} یافت نشد", "NOT_FOUND")
        {
        }
    }
}