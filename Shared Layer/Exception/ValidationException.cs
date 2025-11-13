namespace Shared.Icp.Exceptions
{
    /// <summary>
    /// Custom exception type used to represent validation errors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extends <see cref="BaseException"/> and provides a structured container for field-level validation errors via
    /// <see cref="Errors"/>. The base error <see cref="BaseException.Code"/> is fixed to "VALIDATION_ERROR".
    /// </para>
    /// <para>
    /// Localization: although this type is documented in English, any user-facing texts (e.g., error messages supplied
    /// in <see cref="Errors"/>) should remain Persian for presentation to end users. The constructor overloads in this class
    /// also preserve Persian default messages where applicable.
    /// </para>
    /// <para>
    /// Usage guidance:
    /// - Use <see cref="ValidationException(string)"/> to raise a validation error with a custom (Persian) message and no field map.
    /// - Use <see cref="ValidationException(System.Collections.Generic.Dictionary{string, string[]})"/> when you already have
    ///   a map of field names to an array of messages.
    /// - Use <see cref="ValidationException(string, string)"/> to quickly raise an error for a single field.
    /// - Use <see cref="ValidationException(System.Collections.Generic.Dictionary{string, System.Collections.Generic.List{string}})"/>
    ///   to provide a list-based map that will be converted to arrays.
    /// </para>
    /// <para>
    /// Thread-safety and mutability: the exception object is not intended to be mutated after construction. While
    /// <see cref="Errors"/> exposes a mutable dictionary reference, consumers should treat it as read-only. If callers must
    /// modify the collection, consider cloning to avoid side effects.
    /// </para>
    /// <para>
    /// Serialization: when serialized to JSON, <see cref="Errors"/> is typically emitted as an object mapping field names
    /// to arrays of Persian error messages, e.g. { "Name": ["نام الزامی است"] }.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: collecting multiple field errors and throwing the exception
    /// var errors = new Dictionary&lt;string, string[]&gt;
    /// {
    ///     ["Name"] = new[] { "نام پروژه الزامی است" },
    ///     ["Status"] = new[] { "وضعیت نامعتبر است" }
    /// };
    /// throw new ValidationException(errors);
    ///
    /// // Example: single-field error
    /// throw new ValidationException("Name", "طول نام بیش از حد مجاز است");
    /// </code>
    /// </example>
    /// <seealso cref="BaseException"/>
    /// <seealso cref="NotFoundException"/>
    /// <seealso cref="BusinessRuleException"/>
    /// <seealso cref="FileProcessingException"/>
    public class ValidationException : BaseException
    {
        /// <summary>
        /// Gets the collection of validation errors keyed by field name.
        /// </summary>
        /// <remarks>
        /// The key is the field/property name, and the value is an array of error messages for that field. Messages
        /// should be provided in Persian for user presentation. The collection is initialized by the constructors and
        /// is read-only by reference (the dictionary instance itself cannot be replaced). This property is never null.
        /// </remarks>
        /// <value>
        /// A dictionary mapping field names to arrays of error message strings.
        /// </value>
        public Dictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a custom message.
        /// </summary>
        /// <remarks>
        /// The <see cref="BaseException.Code"/> is set to "VALIDATION_ERROR". The <see cref="Errors"/> map is initialized empty.
        /// </remarks>
        /// <param name="message">Custom validation message (recommended to be Persian for user-facing contexts).</param>
        public ValidationException(string message)
            : base(message, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a predefined set of field errors.
        /// </summary>
        /// <remarks>
        /// Uses a default Persian message and sets the <see cref="BaseException.Code"/> to "VALIDATION_ERROR".
        /// </remarks>
        /// <param name="errors">A dictionary of field names to arrays of error messages (strings in Persian recommended). Must not be null.</param>
        public ValidationException(Dictionary<string, string[]> errors)
            : base("یک یا چند خطای اعتبارسنجی رخ داده است", "VALIDATION_ERROR")
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class for a single field error.
        /// </summary>
        /// <remarks>
        /// Sets a Persian message including the supplied error and sets the <see cref="BaseException.Code"/> to "VALIDATION_ERROR".
        /// </remarks>
        /// <param name="field">The field/property name associated with the error. Must not be null.</param>
        /// <param name="error">The validation error message (Persian recommended for display). Must not be null.</param>
        public ValidationException(string field, string error)
            : base($"خطای اعتبارسنجی: {error}", "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class from a list-based field error map.
        /// </summary>
        /// <remarks>
        /// Converts each <c>List&lt;string&gt;</c> of messages into a <c>string[]</c> to standardize the payload shape for APIs.
        /// Uses a default Persian message and sets the <see cref="BaseException.Code"/> to "VALIDATION_ERROR".
        /// </remarks>
        /// <param name="errors">A dictionary of field names to lists of error messages to be converted to arrays. Must not be null.</param>
        public ValidationException(Dictionary<string, List<string>> errors)
            : base("یک یا چند خطای اعتبارسنجی رخ داده است", "VALIDATION_ERROR")
        {
            Errors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray()
            );
        }
    }
}