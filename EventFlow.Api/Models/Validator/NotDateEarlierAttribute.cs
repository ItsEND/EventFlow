using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EventFlow.Api.Models.Validator;

public class NotDateEarlierAttribute : ValidationAttribute
{
    private readonly string _otherPropertyName;

    public NotDateEarlierAttribute(string otherProperty)
    {
        _otherPropertyName = otherProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success; // null через Required проверяется
        }

        PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);

        if (otherProperty is null)
        {
            return new ValidationResult($"Свойство '{_otherPropertyName}' не найдено.");
        }

        object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        if (otherProperty is null)
        {
            return ValidationResult.Success; //null через Required
        }

        return value is not DateTime currentDate
            ? new ValidationResult("Атрибут можно использовать только для DateTime.")
            : otherValue is not DateTime otherDate
            ? new ValidationResult($"Свойство '{_otherPropertyName}' не является DateTime.")
            : currentDate.Date < otherDate.Date
            ? new ValidationResult(ErrorMessage ?? "Дата события не может быть меньше текущей даты")
            : ValidationResult.Success;
    }
}

