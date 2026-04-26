using EventFlow.Api.Models.Validator;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Contracts.Events;

/// <summary>
/// Параметры запроса для получения списка мероприятий
/// с поддержкой фильтрации и пагинации.
/// </summary>
public record class GetEventsQuery
{
    /// <summary>
    /// Фильтр по названию мероприятия.
    /// Выполняет регистронезависимый поиск по частичному совпадению.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Нижняя граница даты начала мероприятия.
    /// Возвращаются события, начинающиеся не раньше указанной даты.
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// Верхняя граница даты окончания мероприятия.
    /// Возвращаются события, заканчивающиеся не позже указанной даты.
    /// </summary>
    [EndAfterStart(nameof(From), ErrorMessage = "Дата окончания должна быть позже даты начала")]
    public DateTime? To { get; init; }

    /// <summary>
    /// Номер страницы, которую необходимо вернуть.
    /// Значение по умолчанию — 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Страница не может быть меньше 1")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Количество элементов на одной странице.
    /// Значение по умолчанию — 10.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Элементов на странице не может быть меньше 1")]
    public int PageSize { get; init; } = 10;
}
