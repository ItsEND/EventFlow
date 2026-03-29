using EventFlow.Api.Models.Validator;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api.Models;

public class Event
{
    public required Guid Id { get; init; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Название обязательно для заполнения")]
    public required string Title { get; set; }

    [StringLength(400, MinimumLength = 2, ErrorMessage = "Минимальная длина описания 2 символа, максимальная длина не может превышать 400 символов")]
    public string? Description { get; set; }

    [Required]
    [NotPastDate(ErrorMessage = "Дата должна быть сегодня или позже")]
    public required DateTime StartAt { get; set; }

    [Required]
    [NotPastDate(ErrorMessage = "Дата должна быть сегодня или позже")]
    public required DateTime EndAt { get; set; }
}
