using System.Text.Json;
using FluentAssertions;
using JLib.ValueTypes.Mapping.SystemTextJson;
using Xunit;

namespace JLib.ValueTypes.Mapping.Tests;

public class SystemTextJsonConverterTests
{
    record StringVt(string Value) : StringValueType(Value);
    record GuidVt(Guid Value) : GuidValueType(Value);
    record IntVt(int Value) : IntValueType(Value);

    [Fact]
    public void String_Deserialize()
    {
        var value = JsonSerializer.Deserialize<StringVt>("\"description\"",
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().BeOfType<StringVt>();
        value?.Value.Should().BeEquivalentTo("description");
    }
    [Fact]
    public void String_Serialize()
    {
        var value = JsonSerializer.Serialize(new StringVt("description"),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().Be("\"description\"");
    }
    [Fact]
    public void Guid_Deserialize()
    {
        var raw = Guid.NewGuid();
        var value = JsonSerializer.Deserialize<GuidVt>(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().BeOfType<GuidVt>();
        value?.Value.Should().Be(raw);
    }
    [Fact]
    public void Guid_Serialize()
    {
        var raw = Guid.NewGuid();
        var value = JsonSerializer.Serialize(new GuidVt(raw),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().Be($"\"{raw}\"");
    }
    [Fact]
    public void Int_Deserialize()
    {
        var raw = 5;
        var value = JsonSerializer.Deserialize<IntVt>(raw.ToString(),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().BeOfType<IntVt>();
        value?.Value.Should().Be(raw);
    }
    [Fact]
    public void Int_Serialize()
    {
        var raw = 5;
        var value = JsonSerializer.Serialize(new IntVt(raw),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().Be(raw.ToString());
    }
    [Fact]
    public void Dict_Serialize()
    {
        var raw = new Dictionary<IntVt, StringVt>()
        {
            {
                new(1),
                new("one")
            }
        };
        var value = JsonSerializer.Serialize(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().Be("{\"1\":\"one\"}");
    }
    [Fact]
    public void Dict_Deserialize()
    {
        const string raw = "{\"1\":\"one\"}";
        var value = JsonSerializer.Deserialize<Dictionary<IntVt,StringVt>>(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory() }
            });
        value.Should().BeOfType<Dictionary<IntVt, StringVt>>();
        value.Should().HaveCount(1);
        value?[new(1)].Value.Should().Be("one");
    }


}
