namespace EventFlow.Api.Contracts;

/// <summary>
/// Представляет постраничный результат выборки данных.
/// Содержит элементы текущей страницы и информацию о параметрах пагинации.
/// </summary>
/// <typeparam name="T">Тип элементов в результирующей коллекции.</typeparam>
public record PaginatedResult<T>
{
    /// <summary>
    /// Элементы текущей страницы.
    /// </summary>
    public IEnumerable<T> Items { get; init; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    public int CurrentPage { get; init; } = 1;

    /// <summary>
    /// Количество элементов на странице.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Общее количество страниц, рассчитанное на основе общего числа элементов и размера страницы.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Общее количество элементов во всей выборке без учета пагинации.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Количество элементов, фактически возвращенных на текущей странице.
    /// </summary>
    public int TotalItemsOnPage { get; init; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="PaginatedResult{T}"/>.
    /// </summary>
    /// <param name="items">Элементы текущей страницы.</param>
    /// <param name="currentPage">Номер текущей страницы.</param>
    /// <param name="pageSize">Количество элементов на странице.</param>
    /// <param name="totalPages">Общее количество страниц.</param>
    /// <param name="totalItems">Общее количество элементов.</param>
    /// <param name="totalItemsOnPage">Количество элементов на текущей странице.</param>
    public PaginatedResult(
        IEnumerable<T> items,
        int currentPage,
        int pageSize,
        int totalPages,
        int totalItems,
        int totalItemsOnPage)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = totalPages;
        TotalItems = totalItems;
        TotalItemsOnPage = totalItemsOnPage;
    }
}