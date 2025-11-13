using Shared.Icp.Exceptions;
using System.Net;
using System.Text.Json;

namespace Presentation.Icp.API.Middleware
{
    /// <summary>
    /// Standardized error response payload returned by the API on failures.
    /// </summary>
    /// <remarks>
    /// Use this model to provide a consistent error contract to clients. User-facing messages
    /// should remain localized in Persian where applicable. For validation errors, populate the
    /// <see cref="Errors"/> dictionary with field names as keys and arrays of messages as values.
    /// </remarks>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful. Always false for errors.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a human-readable error message (localized in Persian for end users).
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional dictionary of validation errors. The key is the field name
        /// and the value is an array of error messages for that field.
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Creates a generic error response with the specified message and optional validation errors.
        /// </summary>
        /// <param name="message">The error message to display (Persian-localized as needed).</param>
        /// <param name="errors">Optional validation errors keyed by field name.</param>
        /// <returns>An <see cref="ErrorResponse"/> instance with <see cref="Success"/> set to false.</returns>
        public static ErrorResponse Create(string message, Dictionary<string, string[]>? errors = null)
        {
            return new ErrorResponse
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        /// <summary>
        /// Creates a default internal server error response with a standard Persian message.
        /// </summary>
        /// <returns>An <see cref="ErrorResponse"/> instance representing a server error.</returns>
        public static ErrorResponse CreateServerError()
        {
            return new ErrorResponse
            {
                Success = false,
                Message = "خطای سرور رخ داده است"
            };
        }
    }
}