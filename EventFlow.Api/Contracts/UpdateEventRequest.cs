using EventFlow.Api.Models.Validator;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Contracts;

public record class UpdateEventRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Название обязательно для заполнения")]
    public required string Title { get; init; }

    [StringLength(400, ErrorMessage = "Максимальная длина описания не может превышать 400 символов")]
    public string? Description { get; init; }

    [Required(ErrorMessage = "Дата начала обязательна")]
    public required DateTime StartAt { get; init; }

    [Required(ErrorMessage = "Дата окончания обязательна")]
    [EndAfterStart(nameof(StartAt), ErrorMessage = "Дата окончания должна быть позже даты начала")]
    public required DateTime EndAt { get; init; }
}
