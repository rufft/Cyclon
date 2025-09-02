using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Batch.Extensions.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class CornerFormatAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) 
            return ValidationResult.Success;

        if (value is not IEnumerable outer)
            return new ValidationResult("Неверный формат: ожидается массив массивов целых чисел.");

        foreach (var innerObj in outer)
        {
            if (innerObj is not IEnumerable inner)
                return new ValidationResult("Неверный формат: каждый элемент должен быть массивом целых чисел.");

            using var e = inner.Cast<int>().GetEnumerator();
            if (!e.MoveNext())
                continue;

            var prev = e.Current;
            while (e.MoveNext())
            {
                var cur = e.Current;
                if (prev < cur)
                    return new ValidationResult("Каждый подмассив должен быть по убыванию (a[i] >= a[i+1]).");
                prev = cur;
            }
        }

        return ValidationResult.Success;
    }
}