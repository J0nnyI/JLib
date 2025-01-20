using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

internal sealed class TypeValidationContext : ValidationContext<Type>
{
    private readonly TypeValueType _valueType;

    public TypeValidationContext(TypeValueType valueType, Type targetType) : base(valueType.Value, targetType)
    {
        _valueType = valueType;
    }

    protected override string GetExceptionMessage()
        => $"{_valueType.Value.FullName(true)} is not a valid {_valueType.GetType().FullName(true)}";
}