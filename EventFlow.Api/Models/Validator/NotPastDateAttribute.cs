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
        if (value is DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "Дата события не может быть меньше текущей даты");
            }
            return ValidationResult.Success;
        }
        return new ValidationResult("Можно использовать только дату");
    }
}
