using FluentAssertions;
using JLib.ValueTypes.Mapping.NewtonsoftJson;
using Newtonsoft.Json;
using Xunit;

namespace JLib.ValueTypes.Mapping.Tests;

public class NewtonsoftJsonConverterTests
{
    record StringVt(string Value) : StringValueType(Value);
    record GuidVt(Guid Value) : GuidValueType(Value);
    record IntVt(int Value) : IntValueType(Value);

    [Fact]
    public void String_DeserializeObject()
    {
        var value = JsonConvert.DeserializeObject<StringVt>("\"description\"",
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().BeOfType<StringVt>();
        value?.Value.Should().BeEquivalentTo("description");
    }
    [Fact]
    public void String_SerializeObject()
    {
        var value = JsonConvert.SerializeObject(new StringVt("description"),
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().Be("\"description\"");
    }
    [Fact]
    public void Guid_DeserializeObject()
    {
        var raw = Guid.NewGuid();
        var value = JsonConvert.DeserializeObject<Dictionary<string, GuidVt>>(@$"{{""x"":""{raw}""}}",
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().NotBeNull();
        value.Should().ContainKey("x");
        value!.GetValueOrDefault("x").Should().BeOfType<GuidVt>();
        value!["x"].Value.Should().Be(raw);
    }
    [Fact]
    public void Guid_SerializeObject()
    {
        var raw = Guid.NewGuid();
        var value = JsonConvert.SerializeObject(new GuidVt(raw),
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().Be($"\"{raw}\"");
    }
    [Fact]
    public void Int_DeserializeObject()
    {
        var value = JsonConvert.DeserializeObject<Dictionary<string, IntVt>>(@"{""x"":5}",
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().NotBeNull();
        value.Should().ContainKey("x");
        value!.GetValueOrDefault("x").Should().BeOfType<IntVt>();
        value!["x"].Value.Should().Be(5);
    }
    [Fact]
    public void Int_SerializeObject()
    {
        var raw = 5;
        var value = JsonConvert.SerializeObject(new IntVt(raw),
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().Be(raw.ToString());
    }
#if(false)
    [Fact (Skip = "niy")]
    public void Dict_SerializeObject()
    {
        var raw = new Dictionary<IntVt, StringVt>()
        {
            {
                new(1),
                new("one")
            }
        };
        var value = JsonConvert.SerializeObject(raw,
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter() }
            });
        value.Should().Be("{\"1\":\"one\"}");
    }
    [Fact(Skip = "niy")]
    public void Dict_DeserializeObjectIntKey()
    {
        const string raw = "{1:\"one\"}";
        var value = JsonConvert.DeserializeObject<Dictionary<IntVt, StringVt>>(raw,
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(), new ValueTypeDictionaryJsonConverter() }
            });
        value.Should().BeOfType<Dictionary<IntVt, StringVt>>();
        value.Should().HaveCount(1);
        value?[new(1)].Value.Should().Be("one");
    }
    [Fact(Skip = "niy")]
    public void Dict_DeserializeObjectStrKey()
    {
        const string raw = "{\"1\":\"one\"}";
        var value = JsonConvert.DeserializeObject<Dictionary<IntVt, StringVt>>(raw,
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(), new ValueTypeDictionaryJsonConverter() }
            });
        value.Should().BeOfType<Dictionary<IntVt, StringVt>>();
        value.Should().HaveCount(1);
        value?[new(1)].Value.Should().Be("one");
    }
#endif

}
