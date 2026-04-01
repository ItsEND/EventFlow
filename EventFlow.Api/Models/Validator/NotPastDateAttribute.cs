using System.ComponentModel.DataAnnotations;
namespace EventFlow.Api.Models.Validator;

/// <summary>
/// Проверяет что дата не меньше текущей.
/// </summary>
public class NotPastDateAttribute : ValidationAttribute
{
    /// <summary>
    /// Выполняет проверку значения свойства.
    /// </summary>
    /// <param name="value">Значение конечной даты.</param>
    /// <param name="validationContext">Контекст валидации.</param>
    /// <returns>Результат валидации.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success; // null через Required проверяется
        }
        return value is DateTime date
            ? date.Date < DateTime.Today
                ? new ValidationResult(ErrorMessage ?? "Дата события не может быть меньше текущей даты")
                : ValidationResult.Success
            : new ValidationResult("Можно использовать только дату");
    }
}
