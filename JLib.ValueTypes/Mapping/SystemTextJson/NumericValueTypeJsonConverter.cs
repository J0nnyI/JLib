using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using JLib.Helper;

namespace JLib.ValueTypes.Mapping.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types
/// <list type="bullet">
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
internal class NumericValueTypeJsonConverter<TVt, TValue> : JsonConverter<TVt>
    where TVt : ValueType<TValue>
    where TValue:INumber<TValue>
{
    public override TVt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TValue value = typeof(TVt)
                .GetAnyBaseType<ValueType<Ignored>>()
                ?.GenericTypeArguments.First()
            switch
        {
            { } type when type == typeof(byte) => reader.GetByte().CastTo<TValue>(),
            { } type when type == typeof(sbyte) => reader.GetSByte().CastTo<TValue>(),
            { } type when type == typeof(short) => reader.GetUInt16().CastTo<TValue>(),
            { } type when type == typeof(ushort) => reader.GetInt16().CastTo<TValue>(),
            { } type when type == typeof(int) => reader.GetInt32().CastTo<TValue>(),
            { } type when type == typeof(uint) => reader.GetUInt32().CastTo<TValue>(),
            { } type when type == typeof(long) => reader.GetInt64().CastTo<TValue>(),
            { } type when type == typeof(ulong) => reader.GetUInt64().CastTo<TValue>(),
            { } type when type == typeof(decimal) => reader.GetDecimal().CastTo<TValue>(),
            { } type when type == typeof(double) => reader.GetDouble().CastTo<TValue>(),
            { } type when type == typeof(float) => reader.GetSingle().CastTo<TValue>(),
            _ => throw new NotSupportedException(
                $"Type {typeof(TVt).FullName} is not supported. Only Numbers are supported")
        };
        return ValueType.CreateNullable<TVt, TValue>(value);
    }

    public override void Write(Utf8JsonWriter writer, TVt? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (value.Value)// required to select the correct overload
        {
            case byte v:
                writer.WriteNumberValue(v);
                break;
            case sbyte v:
                writer.WriteNumberValue(v);
                break;
            case short v:
                writer.WriteNumberValue(v);
                break;
            case ushort v:
                writer.WriteNumberValue(v);
                break;
            case int v:
                writer.WriteNumberValue(v);
                break;
            case uint v:
                writer.WriteNumberValue(v);
                break;
            case long v:
                writer.WriteNumberValue(v);
                break;
            case ulong v:
                writer.WriteNumberValue(v);
                break;
            case decimal v:
                writer.WriteNumberValue(v);
                break;
            case double v:
                writer.WriteNumberValue(v);
                break;
            case float v:
                writer.WriteNumberValue(v);
                break;
        }

    }
}