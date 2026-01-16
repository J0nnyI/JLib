using System.Text.Json;
using System.Text.Json.Serialization;

namespace JLib.ValueTypes.Mapping.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types
/// <list type="bullet">
/// <item><see cref="Guid"/></item>
/// </list>
/// </summary>
internal class GuidValueTypeJsonConverter<T> : JsonConverter<T>
    where T : ValueType<Guid>?
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            return null;
        var guid = Guid.Parse(value);
        // the nullability error is irrelevant, since it does not change the type argument the return nullability matches the result.
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
        return ValueType.CreateNullable<T,Guid>(guid);
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
    }

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