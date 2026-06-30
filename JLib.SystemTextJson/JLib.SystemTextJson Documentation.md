# JLib.SystemTextJson

System.Text.Json integration for JLib. It automatically discovers `JsonConverter` implementations from the JLib reflection `TypeCache` and registers them with `JsonSerializerOptions`, with support for ordering converters via an attribute and dependency-injection-based configuration.

## Installation
```sh
dotnet add package JLib.SystemTextJson
```

## Features
- Discovers every concrete, non-generic `JsonConverter` implementation through the JLib reflection `ITypeCache` (modelled as the `JsonConverterType` type-value-type).
- Instantiates discovered converters automatically, supporting a parameterless constructor or a single `ITypeCache` constructor argument.
- Registers the discovered converters directly on an `IList<JsonConverter>` or, via dependency injection, on `JsonSerializerOptions`.
- Lets you control the order in which converters are registered with the `ConverterOrderAttribute`.
- Validates that discovered converter types expose a usable constructor and reports actionable errors through the TypeCache validation pipeline.

## Usage

### Registering converters via dependency injection
`AddJsonConverters` configures `JsonSerializerOptions` so that all `JsonConverter` types known to the `ITypeCache` are added. Build a `TypeCache` first (for example with `AddTypeCache` from `JLib.Reflection.DependencyInjection`), then pass it in.

```cs
using JLib.Reflection.DependencyInjection;
using JLib.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// build the reflection TypeCache (scans the given assemblies/prefixes)
services.AddTypeCache(out var typeCache, "MyCompany.MyApp");

// register all discovered JsonConverters onto JsonSerializerOptions
services.AddJsonConverters(typeCache);
```

The converters are applied through `IOptions<JsonSerializerOptions>`, so the configured options can be resolved from the service provider:

```cs
var provider = services.BuildServiceProvider();
var options = provider.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;

var json = JsonSerializer.Serialize(myValue, options);
```

### Adding converters to an existing options instance
If you already have a `JsonSerializerOptions` (or any `IList<JsonConverter>`), use the `AddConverters` extension to append the discovered converters directly.

```cs
using JLib.SystemTextJson;

var options = new JsonSerializerOptions();
options.Converters.AddConverters(typeCache);
```

### Writing a discoverable converter
Any concrete, non-generic type that derives from `JsonConverter` is discovered automatically. The converter must declare either a parameterless constructor or a single constructor taking an `ITypeCache`; otherwise it is skipped and a validation error is reported.

```cs
using System.Text.Json;
using System.Text.Json.Serialization;
using JLib.Reflection;

public sealed class TimeSpanConverter : JsonConverter<TimeSpan>
{
    // parameterless constructor -> discovered automatically
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeSpan.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

// or, when the converter needs reflection metadata:
public sealed class MyTypeCacheAwareConverter : JsonConverter<MyType>
{
    public MyTypeCacheAwareConverter(ITypeCache typeCache) { /* ... */ }
    // Read/Write ...
}
```

### Controlling converter order
System.Text.Json uses the first matching converter, so registration order can matter. Decorate a converter with `ConverterOrderAttribute` to influence where it is placed; lower values are registered first. The default order is `0` (`ConverterOrderAttribute.DefaultOrder`).

```cs
using JLib.SystemTextJson;

[ConverterOrder(-1)] // registered before converters with the default order of 0
public sealed class HighPriorityConverter : JsonConverter<MyType>
{
    // Read/Write ...
}
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — provides the `ITypeCache` and `TypeValueType` infrastructure used to discover converters.
- [JLib.Reflection.DependencyInjection](../JLib.Reflection.DependencyInjection/JLib.Reflection.DependencyInjection%20Documentation.md) — supplies `AddTypeCache` to build the `ITypeCache` consumed by `AddJsonConverters`.
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) — `JsonConverterType` is built on the `TypeValueType` / validated-type model from this package.
