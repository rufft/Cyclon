using System.ComponentModel.DataAnnotations;

namespace Batch.Extensions.Validation;

public class FieldStringLengthAttribute : StringLengthAttribute
{
    public FieldStringLengthAttribute(int maxLength) : base(maxLength)
    {
    }

    public override string FormatErrorMessage(string name)
    {
        return $"Поле не может быть длинее {MaximumLength} символов";
    }
}