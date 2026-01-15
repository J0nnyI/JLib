using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

internal sealed class TypeValidationContext(TypeValueType valueType, Type targetType)
    : ValidationContext<Type>(valueType.Value, targetType)
{
    protected override string GetExceptionMessage()
        => $"{valueType.Value.FullName(true)} is not a valid {valueType.GetType().FullName(true)}";
}