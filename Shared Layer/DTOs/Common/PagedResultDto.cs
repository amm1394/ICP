namespace Shared.Icp.DTOs.Common
{
    /// <summary>
    /// Standard paginated result wrapper returned by list endpoints.
    /// </summary>
    /// <remarks>
    /// Use together with <see cref="PaginationDto"/> to carry both data and paging metadata.
    /// User-facing messages (validation/errors/success) remain localized in Persian elsewhere; this
    /// DTO only transports data. Notes:
    /// - PageNumber is 1-based.
    /// - PageSize should be greater than 0; service layers typically enforce min/max limits.
    /// - TotalPages is computed via Math.Ceiling(TotalCount / PageSize) and yields 0 when PageSize is 0.
    /// - HasPreviousPage/HasNextPage are convenience flags for navigation.
    /// </remarks>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// The items for the current page. Defaults to an empty list.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// The current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of items across all pages (before paging).
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The total number of pages given <see cref="TotalCount"/> and <see cref="PageSize"/>.
        /// Returns 0 when <see cref="PageSize"/> is 0.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Indicates whether a previous page exists (i.e., <see cref="PageNumber"/> &gt; 1).
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indicates whether a next page exists (i.e., <see cref="PageNumber"/> &lt; <see cref="TotalPages"/>).
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}