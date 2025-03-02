using JLib.Helper;

using Newtonsoft.Json;

// Other necessary using directives...

namespace JLib.ValueTypes.Mapping.NewtonsoftJson;
// not implemented yet
#if false
public class ValueTypeDictionaryJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var keyType = objectType.GenericTypeArguments.First();
        var nativeKeyType = objectType.GetAnyBaseType<ValueType<Ignored>>()?.GenericTypeArguments.Single();
        var nativeDictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), objectType.GenericTypeArguments[1]);
        var nativeDict = Activator.CreateInstance(nativeDictType)
            ?? throw new InvalidOperationException("dictionary could not be created");

        serializer.Populate(reader, nativeDict);

        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="JsonConverter.CanConvert"/>
    /// </summary>
    public override bool CanConvert(Type objectType)
        => objectType.IsGenericTypeDefinition
           && objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
           && objectType.GetGenericTypeDefinition().GenericTypeArguments.First() == typeof(ValueType<>);
}
#endif
/// <summary>
/// Allows the <see cref="JsonSerializer"/> to Serialize and Deserialize ValueTypes as if they were native
/// </summary>
public class ValueTypeJsonConverter : JsonConverter
{
    /// <summary>
    /// <inheritdoc cref="JsonConverter.CanConvert"/>
    /// </summary>
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsDerivedFromAny<ValueType<Ignored>>();
    }

    /// <summary>
    /// <inheritdoc cref="JsonConverter.ReadJson"/>
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // todo: add dictionary primary key support
        var nativeType = objectType
            .GetAnyBaseType<ValueType<Ignored>>()
            ?.GenericTypeArguments.First();

        object? value = nativeType
            switch
        {
            not null when reader.Value == null => null,
            not null when nativeType == typeof(Guid) => Guid.Parse((string)reader.Value),
            not null when nativeType == typeof(string) => reader.Value,
            not null when nativeType == typeof(byte) => Convert.ToByte(reader.Value),
            not null when nativeType == typeof(sbyte) => Convert.ToSByte(reader.Value),
            not null when nativeType == typeof(short) => Convert.ToInt16(reader.Value),
            not null when nativeType == typeof(ushort) => Convert.ToUInt16(reader.Value),
            not null when nativeType == typeof(int) => Convert.ToInt32(reader.Value),
            not null when nativeType == typeof(uint) => Convert.ToUInt32(reader.Value),
            not null when nativeType == typeof(long) => Convert.ToInt64(reader.Value),
            not null when nativeType == typeof(ulong) => Convert.ToUInt64(reader.Value),
            not null when nativeType == typeof(decimal) => Convert.ToDecimal(reader.Value),
            not null when nativeType == typeof(double) => Convert.ToDouble(reader.Value),
            not null when nativeType == typeof(float) => Convert.ToSingle(reader.Value),
            _ => throw new NotSupportedException(
                $"Type {objectType.FullName()} is not supported. Only Numbers are supported")
        };
        return ValueType.CreateNullable(objectType, value);

    }

    /// <summary>
    /// <inheritdoc cref="JsonConverter.WriteJson"/>
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value?.GetType().IsDerivedFromAny<ValueType<Ignored>>() == false)
            throw new NotSupportedException($"Type {value.GetType().FullName()} is not supported");

        writer.WriteValue(value?.GetType().GetProperty(nameof(ValueType<Ignored>.Value))?.GetValue(value));
    }

}