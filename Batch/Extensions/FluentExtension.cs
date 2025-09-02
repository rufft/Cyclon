namespace Batch.Extensions;

public class FluentExtension
{
    public static void UpdateEntityProperties<T>(T entity, object input)
    {
        foreach (var property in input.GetType().GetProperties())
        {
            var value = property.GetValue(input);
            if (value is not Optional<object> { HasValue: true } optional) continue;
            var entityProperty = entity?.GetType().GetProperty(property.Name);
            
            if (entityProperty == null || !entityProperty.CanWrite) continue;
            var convertedValue = Convert.ChangeType(optional.Value, entityProperty.PropertyType);
            
            entityProperty.SetValue(entity, convertedValue);
        }
    }
}