namespace EventFlow.Api.Contracts;

public record PaginatedResult<T>
{
    public IEnumerable<T> Items {get; init;}
    public int CurrentPage { get; init;}
    public int TotalPages { get; init;}
    public int TotalItems { get; init;}
    
    public PaginatedResult(IEnumerable<T> items, int currentPage, int totalPages, int totalItems)
    {
        Items = items;
        CurrentPage = currentPage;
        TotalPages = totalPages;
        TotalItems = totalItems;
    }
    

}