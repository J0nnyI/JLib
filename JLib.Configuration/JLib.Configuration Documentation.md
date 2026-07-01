# Project. JLib.Configuration
provides reflection based config section access either direct or via dependency injection.

A config section is any class marked with the `[ConfigSectionName]` attribute. The
section key is declared once, on the type, and everything else derives from it.

```cs
[ConfigSectionName("Test")]
public class MyConfigSection
{
    public string MyValue { get; init; }
}
```

Every registration binds the section through the standard options pattern
(`services.Configure<T>`), so it becomes available as `IOptions<T>`,
`IOptionsSnapshot<T>` and `IOptionsMonitor<T>` — the consuming code picks the
update/reload semantics it needs. JLib never hides that choice.

## Registering a single config section

```cs
using JLib.Configuration;
using Microsoft.Extensions.Options;

IConfiguration myConfig = ...;

var services = new ServiceCollection()
    .AddConfigSection<MyConfigSection>(myConfig);

using var provider = services.BuildServiceProvider();
var section = provider.GetRequiredService<IOptions<MyConfigSection>>().Value;
```

`AddConfigSection<T>` is strongly typed and resolves the section name from the
attribute — no string keys at the call site.

## Registering all config sections

`AddAllConfigSections` discovers every `[ConfigSectionName]` type via the type cache
and binds each one, so sections never have to be registered individually.

```cs
using JLib.Configuration;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.Options;

IConfiguration myConfig = ...;

var services = new ServiceCollection()
    .AddTypeCache(out var typeCache, nameof(JLib))
    .AddAllConfigSections(typeCache, myConfig);

using var provider = services.BuildServiceProvider();
var section = provider.GetRequiredService<IOptions<MyConfigSection>>().Value;
```

When using the generic host, the `JLib.AspNetCore` package offers an
`IHostApplicationBuilder` overload that pulls configuration and services from the
builder:

```cs
using JLib.AspNetCore;

builder.AddAllConfigSections(typeCache); // reads builder.Configuration, registers into builder.Services
```

## Directly retrieving config section objects

```cs
using JLib.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

IConfiguration myConfig = ...;
ILoggerFactory loggerFactory = NullLoggerFactory.Instance; // or your own

var configSection = myConfig.GetSectionObject<MyConfigSection>(loggerFactory);

[ConfigSectionName("Test")]
public class MyConfigSection
{
    public string MyValue { get; init; }
}
```
## Environment specific configuration

Environment specific configuration is no longer handled by JLib. Use the standard
.NET configuration providers and layer environment specific config section files on
top of the base configuration when building the `IConfiguration`, e.g.:

```cs
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .Build();
```

Sections from later sources override matching keys from earlier ones, so the
environment specific file only needs to contain the values that differ.