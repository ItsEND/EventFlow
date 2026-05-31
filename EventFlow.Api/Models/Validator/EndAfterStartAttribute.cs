using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EventFlow.Api.Models.Validator;

/// <summary>
/// Проверяет, что значение конечной даты больше значения начальной даты.
/// </summary>
public class EndAfterStartAttribute : ValidationAttribute
{
    private readonly string _startPropertyName;

    /// <summary>
    /// Инициализирует атрибут проверки конечной даты относительно начальной.
    /// </summary>
    /// <param name="startPropertyName">Имя свойства с начальной датой.</param>
    public EndAfterStartAttribute(string startPropertyName)
    {
        _startPropertyName = startPropertyName;
    }

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

