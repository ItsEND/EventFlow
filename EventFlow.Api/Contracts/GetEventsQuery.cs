using EventFlow.Api.Models.Validator;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Contracts;

public record class GetEventsQuery
{
    public string? Title { get; init; }

    public DateTime? From { get; init; }
    [EndAfterStart(nameof(From), ErrorMessage = "Дата окончания должна быть позже даты начала")]
    public DateTime? To { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Страница не может быть < 1")]
    public int Page { get; init; } = 1;
    [Range(1, 50, ErrorMessage = "Элементов на странице не может быть меньше 1 и больше 50")]
    public int PageSize { get; init; } = 10;
}

