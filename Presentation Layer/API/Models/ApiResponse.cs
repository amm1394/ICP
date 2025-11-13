namespace Presentation.Icp.API.Models
{
    /// <summary>
    /// Represents a standard envelope for API responses, providing a consistent structure
    /// for success/failure indication, message, payload, and optional validation errors.
    /// </summary>
    /// <typeparam name="T">The type of the data payload returned by the API.</typeparam>
    /// <remarks>
    /// Use this model for all controller responses to keep a uniform contract across the API.
    /// Notes:
    /// - User-facing messages are localized in Persian when applicable.
    /// - For validation errors, populate the <see cref="Errors"/> dictionary with field names as keys and
    ///   arrays of messages as values.
    /// Example successful payload:
    /// { "success": true, "message": "عملیات با موفقیت انجام شد", "data": { ... } }
    /// Example failed payload:
    /// { "success": false, "message": "پیام خطا", "errors": { "Field": ["خطا۱", "خطا۲"] } }
    /// </remarks>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets an optional human-readable message describing the outcome.
        /// Note: User-facing messages are localized in Persian when applicable.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the data payload for successful responses. This is null for failures.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of validation errors where the key is a field name and the value
        /// is an array of error messages for that field. Present only for validation failures.
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Creates a successful <see cref="ApiResponse{T}"/> with the provided data and optional message.
        /// </summary>
        /// <param name="data">The data payload to include in the response.</param>
        /// <param name="message">An optional success message. Defaults to a localized Persian message.</param>
        /// <returns>A successful API response containing the provided data.</returns>
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message ?? "عملیات با موفقیت انجام شد",
                Data = data
            };
        }

        /// <summary>
        /// Creates a failed <see cref="ApiResponse{T}"/> with the provided message and optional validation errors.
        /// </summary>
        /// <param name="message">A human-readable error message (localized as needed, typically Persian).</param>
        /// <param name="errors">An optional dictionary of validation errors keyed by field name.</param>
        /// <returns>A failed API response containing the provided message and errors.</returns>
        public static ApiResponse<T> FailureResponse(string message, Dictionary<string, string[]>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}