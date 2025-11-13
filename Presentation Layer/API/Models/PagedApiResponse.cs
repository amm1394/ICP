namespace Presentation.Icp.API.Models
{
    /// <summary>
    /// Represents a standard envelope for paged API responses, including pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the paged data set.</typeparam>
    /// <remarks>
    /// Use this model to return list endpoints with pagination information in a consistent format.
    /// Example successful payload:
    /// { "success": true, "message": "فهرست با موفقیت بازیابی شد", "data": [ ... ], "totalCount": 123, "pageNumber": 2, "pageSize": 25, "totalPages": 5, "hasPrevious": true, "hasNext": true }
    /// </remarks>
    public class PagedApiResponse<T>
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
        /// Gets or sets the paged data items.
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of items available across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of pages based on <see cref="TotalCount"/> and <see cref="PageSize"/>.
        /// Returns 0 when <see cref="PageSize"/> is 0.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Gets a value indicating whether there exists a previous page (i.e., <see cref="PageNumber"/> &gt; 1).
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Gets a value indicating whether there exists a next page (i.e., <see cref="PageNumber"/> &lt; <see cref="TotalPages"/>).
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;

        /// <summary>
        /// Creates a successful paged response with Persian default message.
        /// </summary>
        /// <param name="items">The data items for the current page.</param>
        /// <param name="totalCount">The total number of items across all pages.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="message">Optional success message. Defaults to a Persian message.</param>
        /// <returns>A populated <see cref="PagedApiResponse{T}"/> with Success=true.</returns>
        public static PagedApiResponse<T> SuccessResponse(
            List<T> items,
            int totalCount,
            int pageNumber,
            int pageSize,
            string? message = null)
        {
            return new PagedApiResponse<T>
            {
                Success = true,
                Message = message ?? "عملیات با موفقیت انجام شد",
                Data = items ?? new List<T>(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Creates a failed paged response with a Persian-capable error message.
        /// </summary>
        /// <param name="message">The error message to display (localized as needed).</param>
        /// <param name="pageNumber">Optional page number context (defaults to 1).</param>
        /// <param name="pageSize">Optional page size context (defaults to 0).</param>
        /// <returns>A populated <see cref="PagedApiResponse{T}"/> with Success=false and empty data.</returns>
        public static PagedApiResponse<T> FailureResponse(
            string message,
            int pageNumber = 1,
            int pageSize = 0)
        {
            return new PagedApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = new List<T>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}