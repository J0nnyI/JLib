using System.Text.Json;
using System.Text.Json.Serialization;

namespace JLib.ValueTypes.Mapping.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types
/// <list type="bullet">
/// <item><see cref="string"/></item>
/// </list>
/// </summary>
internal class StringValueTypeJsonConverter<T> : JsonConverter<T>
    where T: ValueType<string>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
        => ValueType.CreateNullable<T,string>(reader.GetString());

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }
}