using EventFlow.Api.Models.Validator;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Contracts;

public class EventRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Название обязательно для заполнения")]
    public required string Title { get; set; }

    [StringLength(400, MinimumLength = 2, ErrorMessage = "Минимальная длина описания 2 символа, максимальная длина не может превышать 400 символов")]
    public string? Description { get; set; }

    [Required]
    [NotPastDate(ErrorMessage = "Дата должна быть сегодня или позже")]
    public required DateTime StartAt { get; set; }

    [Required]
    [NotDateEarlier(nameof(StartAt), ErrorMessage = "Дата окончания события не может быть меньше даты начала события")]
    public required DateTime EndAt { get; set; }
}
