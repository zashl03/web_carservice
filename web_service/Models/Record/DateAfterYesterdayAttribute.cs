using System;
using System.ComponentModel.DataAnnotations;

public class DateAfterYesterdayAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            // Если дата не обязательна, удалите эту проверку
            return new ValidationResult("Дата не указана.");
        }

        if (value is DateTime dateValue)
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);

            if (dateValue.Date > yesterday)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(ErrorMessage ?? "Дата должна быть позже вчерашнего дня.");
            }
        }

        return new ValidationResult("Некорректный формат даты.");
    }
}