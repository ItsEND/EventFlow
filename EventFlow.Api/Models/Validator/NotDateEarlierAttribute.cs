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

        if (value is not DateTime currentDate)
        {
            return new ValidationResult("Атрибут можно использовать только для DateTime.");
        }

        if (otherValue is not DateTime otherDate)
        {
            return new ValidationResult($"Свойство '{_otherPropertyName}' не является DateTime.");
        }
        if (currentDate.Date < otherDate.Date)

        {
            return new ValidationResult(ErrorMessage ?? "Дата события не может быть меньше текущей даты");
        }

        return ValidationResult.Success;
    }
}

