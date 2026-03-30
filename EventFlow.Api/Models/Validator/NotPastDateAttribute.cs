using System.ComponentModel.DataAnnotations;
namespace EventFlow.Api.Models.Validator;

public class NotPastDateAttribute : ValidationAttribute
{
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
