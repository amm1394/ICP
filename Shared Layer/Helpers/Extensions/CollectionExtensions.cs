namespace Shared.Icp.Helpers.Extensions
{
    /// <summary>
    /// Extension methods that provide common helpers for working with collection types such as
    /// <see cref="IEnumerable{T}"/> and <see cref="ICollection{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These helpers are designed to simplify frequent collection operations (null/empty checks, pagination,
    /// joining, bulk add/remove). Methods are implemented as pure extension methods and do not mutate inputs
    /// unless explicitly documented (e.g., <see cref="AddRange{T}(ICollection{T}, IEnumerable{T})"/>, 
    /// <see cref="RemoveWhere{T}(ICollection{T}, System.Func{T, bool})"/>).
    /// </para>
    /// <para>
    /// Performance and safety notes:
    /// - Methods that iterate (e.g., pagination, join, remove) enumerate the source sequence at least once.
    /// - No argument validation beyond null checks is performed; callers should ensure valid ranges (e.g., pageSize &gt; 0).
    /// - For potentially large sequences, consider deferred execution implications and multiple enumeration costs.
    /// </para>
    /// </remarks>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the sequence is <c>null</c> or contains no elements; otherwise, <c>false</c>.
        /// </summary>
        /// <typeparam name="T">Element type of the collection.</typeparam>
        /// <param name="collection">The sequence to test; may be <c>null</c>.</param>
        /// <returns>
        /// <c>true</c> when <paramref name="collection"/> is <c>null</c> or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Returns a page of items from the source sequence using 1-based page numbering.
        /// </summary>
        /// <typeparam name="T">Element type of the sequence.</typeparam>
        /// <param name="source">The source sequence to paginate. Must not be <c>null</c>.</param>
        /// <param name="pageNumber">The 1-based page number. Expected to be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items per page. Expected to be greater than 0.</param>
        /// <returns>
        /// A sequence containing at most <paramref name="pageSize"/> items starting at the specified page.
        /// </returns>
        /// <remarks>
        /// Implemented as <c>Skip((pageNumber - 1) * pageSize).Take(pageSize)</c>. No validation is enforced for
        /// negative/zero arguments; invalid values may lead to unexpected results or exceptions from LINQ providers.
        /// The order of items is determined by the order of <paramref name="source"/>.
        /// </remarks>
        public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            return source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Joins the elements of a sequence into a single string using the specified separator.
        /// </summary>
        /// <typeparam name="T">Element type of the sequence.</typeparam>
        /// <param name="collection">The sequence of elements to join. Must not be <c>null</c>.</param>
        /// <param name="separator">The string used as a separator between elements. Defaults to ", ".</param>
        /// <returns>
        /// A concatenated string of element representations separated by <paramref name="separator"/>.
        /// </returns>
        /// <remarks>
        /// Uses <see cref="string.Join(string?, System.Collections.Generic.IEnumerable{string?})"/> semantics: each element is
        /// converted using its <see cref="object.ToString"/>; a <c>null</c> element is represented as an empty string.
        /// </remarks>
        public static string JoinToString<T>(this IEnumerable<T> collection, string separator = ", ")
        {
            return string.Join(separator, collection);
        }

        /// <summary>
        /// Adds a range of items to the target <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="T">Element type of the collection.</typeparam>
        /// <param name="collection">The target collection to modify. Must not be <c>null</c>.</param>
        /// <param name="items">The items to add. Must not be <c>null</c>.</param>
        /// <remarks>
        /// Side effects: modifies <paramref name="collection"/> by appending items in the order provided.
        /// Exceptions thrown by the underlying <see cref="ICollection{T}.Add(T)"/> implementation will surface to the caller.
        /// Duplicates are allowed unless restricted by the concrete collection type.
        /// </remarks>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Removes all items from the target <see cref="ICollection{T}"/> that match a predicate.
        /// </summary>
        /// <typeparam name="T">Element type of the collection.</typeparam>
        /// <param name="collection">The target collection to modify. Must not be <c>null</c>.</param>
        /// <param name="predicate">A function to test each element for a condition. Must not be <c>null</c>.</param>
        /// <remarks>
        /// To avoid modifying the collection during enumeration, the items to remove are first materialized to a temporary list
        /// and then removed. Side effects: modifies <paramref name="collection"/>. For read-only collections, the method will
        /// throw at runtime via the underlying <see cref="ICollection{T}.Remove(T)"/> implementation.
        /// </remarks>
        public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
        }
    }
}