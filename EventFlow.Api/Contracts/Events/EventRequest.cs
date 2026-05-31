using EventFlow.Api.Models.Validator;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Contracts.Events;

/// <summary>
/// Запрос на создание мероприятия.
/// </summary>
public record class EventRequest
{
    /// <summary>
    /// Название мероприятия.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Название обязательно для заполнения")]
    [StringLength(200, ErrorMessage = "Максимальная длина названия не может превышать 200 символов")]
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия.
    /// </summary>
    [StringLength(400, ErrorMessage = "Максимальная длина описания не может превышать 400 символов")]
    public string? Description { get; init; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    [Required(ErrorMessage = "Количество мест обязательно")]
    [Range(1, int.MaxValue, ErrorMessage = "Количество мест должно быть больше нуля")]
    public int? TotalSeats { get; init; }

    /// <summary>
    /// Дата и время начала мероприятия.
    /// </summary>
    [Required(ErrorMessage = "Дата начала обязательна")]
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата и время окончания мероприятия.
    /// </summary>
    [Required(ErrorMessage = "Дата окончания обязательна")]
    [EndAfterStart(nameof(StartAt), ErrorMessage = "Дата окончания должна быть позже даты начала")]
    public required DateTime EndAt { get; init; }
}
