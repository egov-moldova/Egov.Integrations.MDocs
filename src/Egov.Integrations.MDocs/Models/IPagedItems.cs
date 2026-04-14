namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// A list of items that represents a single page of data.
/// </summary>
/// <typeparam name="T">The type of item.</typeparam>
public interface IPagedItems<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    int TotalCount { get; init; }

    /// <summary>
    /// Current page number.
    /// </summary>
    int CurrentPage { get; init; }

    /// <summary>
    /// Current page size.
    /// </summary>
    int PageSize { get; init; }

    /// <summary>
    /// Helper that calculates the total number of pages.
    /// </summary>
    int TotalPages { get; }
}