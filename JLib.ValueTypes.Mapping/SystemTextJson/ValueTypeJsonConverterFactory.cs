using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes.Mapping.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types<br/>
/// uses automapper to instantiate the <see cref="ValueType{T}"/>.<br/>
/// supports <seealso cref="Dictionary{TKey,TValue}"/> conversions where the key is a value type
/// <list type="bullet">
/// <item><see cref="Guid"/></item>
/// <item><see cref="string"/></item>
/// <item><see cref="byte"/></item>
/// <item><see cref="sbyte"/></item>
/// <item><see cref="short"/></item>
/// <item><see cref="ushort"/></item>
/// <item><see cref="int"/></item>
/// <item><see cref="uint"/></item>
/// <item><see cref="long"/></item>
/// <item><see cref="ulong"/></item>
/// <item><see cref="decimal"/></item>
/// <item><see cref="double"/></item>
/// <item><see cref="float"/></item>
/// </list>
/// </summary>
public class ValueTypeJsonConverterFactory : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, JsonConverter> _converters = new();

    /// <summary>
    /// <inheritdoc cref="JsonConverter.CanConvert"/>
    /// </summary>
    public override bool CanConvert(Type typeToConvert)
    {
        return IsValueType(typeToConvert) || IsValueTypeDictionary(typeToConvert);
    }

    /// <summary>
    /// <inheritdoc cref="JsonConverterFactory.CreateConverter"/>
    /// </summary>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => _converters.GetValueOrAdd(typeToConvert, CreateConverter);



    private JsonConverter CreateConverter(Type type)
    {
        if (IsValueType(type))
            return CreateValueTypeConverter(type);
        if(IsValueTypeDictionary(type))
            return CreateValueTypeDictionaryConverter(type);
        throw new NotSupportedException($"Type {type.FullName()} is not supported");
    }

    private bool IsValueType(Type type)
        => type.IsDerivedFromAny<ValueType<Ignored>>();
    private JsonConverter CreateValueTypeConverter(Type typeToConvert)
    {
        Type nativeType = typeToConvert
            .GetAnyBaseType<ValueType<Ignored>>()
            !.GenericTypeArguments
            .Single();

        Type converterType = nativeType switch
        {
            _ when nativeType.IsString() => typeof(StringValueTypeJsonConverter<>).MakeGenericType(typeToConvert),
            _ when nativeType.IsNumber() => typeof(NumericValueTypeJsonConverter<,>).MakeGenericType(typeToConvert, nativeType),
            _ when nativeType.IsGuid() => typeof(GuidValueTypeJsonConverter<>).MakeGenericType(typeToConvert),
            _ => throw new NotSupportedException(
                $"Type {typeToConvert.FullName()} is not supported. Only Numbers, strings and Guids are supported")
        };

        return Activator.CreateInstance(converterType)
                   ?.As<JsonConverter>()
               ?? throw new InvalidOperationException($"Activator failed to create converter of type {converterType.FullName()}");

    }

    private bool IsValueTypeDictionary(Type type)
    {
        var i = type.GetAnyInterface<IDictionary<Ignored, Ignored>>();
        return i is not null
               && i.GenericTypeArguments.First().IsDerivedFromAny<ValueType<Ignored>>();
    }

    private JsonConverter CreateValueTypeDictionaryConverter(Type typeToConvert)
    {
        var args = typeToConvert.GetAnyInterface<IDictionary<Ignored, Ignored>>()?.GenericTypeArguments
            ?? throw new NotSupportedException($"{typeToConvert.FullName(true)} is not a dictionary");
        var keyValueType = args[0];
        var keyNativeType = keyValueType.GetAnyBaseType<ValueType<Ignored>>()?.GenericTypeArguments.Single()
            ?? throw new InvalidSetupException("native type not found");
        var valueType = args[1];
        var converterType = typeof(ValueTypeDictionaryJsonConverter<,,>).MakeGenericType(keyValueType, keyNativeType, valueType);

        return Activator.CreateInstance(converterType)
                   ?.As<JsonConverter>()
               ?? throw new InvalidOperationException($"Activator failed to create converter of type {converterType.FullName()}");
    }

}