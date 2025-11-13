namespace Shared.Icp.DTOs.Common
{
    /// <summary>
    /// Data transfer object for paging and sorting parameters.
    /// </summary>
    /// <remarks>
    /// This DTO is typically bound from query string parameters for list endpoints.
    /// User-facing validation/response messages remain localized in Persian elsewhere; this type only
    /// carries paging/sorting intent. Typical expectations:
    /// - PageNumber starts from 1 (values less than 1 should be normalized to 1 by the caller).
    /// - PageSize greater than 0 (upper bounds may be enforced by the API/service layer).
    /// - SortBy matches a valid field/property name in the target model.
    /// - IsDescending chooses sort direction.
    /// Example: /api/items?pageNumber=2&pageSize=50&sortBy=CreatedAt&isDescending=true
    /// </remarks>
    public class PaginationDto
    {
        /// <summary>
        /// The current page number (1-based). Defaults to 1.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// The number of items per page. Defaults to 20.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// The field/property name to sort by. Case-insensitive depending on implementation. Optional.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Indicates whether the sort order is descending. False means ascending. Defaults to false.
        /// </summary>
        public bool IsDescending { get; set; } = false;
    }
}