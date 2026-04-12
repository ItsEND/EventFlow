namespace EventFlow.Api.Contracts;

public record PaginatedResult<T>
{
    public IEnumerable<T> Items { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalPages { get; init; }
    public int TotalItems { get; init; }
    public int TotalItemsOnPage { get; init; }

    public PaginatedResult(IEnumerable<T> items, int currentPage, int pageSize, int totalPages, int totalItems, int totalItemsOnPage)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = totalPages;
        TotalItems = totalItems;
        TotalItemsOnPage = totalItemsOnPage;
    }
}