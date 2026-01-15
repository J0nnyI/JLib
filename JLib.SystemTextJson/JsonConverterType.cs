using System.Reflection;
using System.Text.Json.Serialization;

using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;

using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.SystemTextJson;

/// <summary>
/// <see cref="TypeValueType"/> for <see cref="JsonConverter{T}"/> implementations.
/// </summary>
[NotGeneric, NotAbstract, IsDerivedFrom(typeof(JsonConverter))]
public record JsonConverterType : TypeValueType, IValidatedType
{
    private JsonConverter? _instance;

    /// <summary>
    /// <inheritdoc cref="JsonConverterType"/>
    /// </summary>
    public JsonConverterType(Type Value) : base(Value)
    {
        _constructor = Value.GetConstructor([])
            ?? Value.GetConstructor([typeof(ITypeCache)]);
    }

    private readonly ConstructorInfo? _constructor;

    /// <summary>
    /// returns a singleton instance of this <see cref="JsonConverterType"/> if it has a parameterless constructor.
    /// </summary>
    /// <returns></returns>
    public JsonConverter? Create(ITypeCache typeCache)
    {
        if (_constructor is null)
            return null;
        // the only possible parameter is typeCache
        var parameters = _constructor.GetParameters().Select(object (_) => typeCache).ToArray();
        return _instance ??= _constructor.Invoke(parameters).CastTo<JsonConverter>();
    }

    void IValidatedType.Validate(ITypeCache cache, IValidationContext<Type> value)
    {
        if (_constructor is null)
            value.AddError($"Constructor could not be found. It has to have either no arguments or a single {typeof(ITypeCache).FullName()} argument.",
                $"If you do not want to create them automatically, decorating them with the '{typeof(IgnoreInCache).FullName(true)}' or removing the types from the type package using a filter will achieve this goal. Note, that this will works by removing them completely from the TypeCache.");
    }

}