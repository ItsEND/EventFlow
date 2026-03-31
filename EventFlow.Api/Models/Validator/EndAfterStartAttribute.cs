using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EventFlow.Api.Models.Validator;

public class EndAfterStartAttribute : ValidationAttribute
{
    private readonly string _startPropertyName;

    public EndAfterStartAttribute(string startPropertyName)
    {
        _startPropertyName = startPropertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        PropertyInfo? startProperty = validationContext.ObjectType.GetProperty(_startPropertyName);

        if (startProperty is null)
        {
            return new ValidationResult($"Свойство '{_startPropertyName}' не найдено.");
        }

        object? startValue = startProperty.GetValue(validationContext.ObjectInstance);

        return startValue is null
            ? ValidationResult.Success
            : value is not DateTime endAt
            ? new ValidationResult("Атрибут можно использовать только для DateTime.")
            : startValue is not DateTime startAt
            ? new ValidationResult($"Свойство '{_startPropertyName}' должно быть DateTime.")
            : endAt <= startAt
            ? new ValidationResult(ErrorMessage ?? "Дата окончания должна быть позже даты начала.")
            : ValidationResult.Success;
    }
}

