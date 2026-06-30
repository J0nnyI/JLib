# JLib.Configuration

Reflection-based configuration section access for `Microsoft.Extensions.Configuration`. Discovers classes marked with `ConfigSectionNameAttribute` via the JLib type cache and registers them in the dependency injection container or retrieves them directly, with environment-aware section selection.

## Installation
```sh
dotnet add package JLib.Configuration
```

## Features
- Mark a plain class with `[ConfigSectionName("...")]` to bind it to a configuration section.
- Discover and register all marked sections at once via `ConfigureAll`, which builds on `Microsoft.Extensions.Options`.
- Inject the bound section type directly (e.g. `MyConfig`) instead of going through `IOptions<MyConfig>`.
- Retrieve a section object directly from an `IConfiguration` without DI using `ConfigurationHelper`.
- Environment-aware section selection: a top-level `Environment` key, or a per-section `Environment` override, picks a nested sub-section to load.

## Usage

### Registering all config sections in the DI container
Add the JLib type cache (from `JLib.Reflection`), then call `ConfigureAll`. Every type marked with `ConfigSectionNameAttribute` is bound and registered so it can be resolved directly.

```cs
using JLib.Configuration;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

var services = new ServiceCollection()
    .AddTypeCache(out var typeCache, "JLib.")
    .ConfigureAll(typeCache, config, loggerFactory, ServiceLifetime.Singleton);

using var provider = services.BuildServiceProvider();

// resolved directly, not wrapped in IOptions<T>
var demo = provider.GetRequiredService<DemoConfig>();

[ConfigSectionName("Demo")]
public class DemoConfig
{
    public string? ConfigProperty { get; init; }
}
```

### Retrieving a section object directly
When you do not use dependency injection, read a marked section straight from an `IConfiguration`.

```cs
using JLib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

IConfiguration config = ...;
var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

DemoConfig? demo = config.GetSectionObject<DemoConfig>(loggerFactory);

[ConfigSectionName("Demo")]
public class DemoConfig
{
    public string? ConfigProperty { get; init; }
}
```

`GetSection<T>(IConfiguration, ILoggerFactory)` is also available when you need the raw `IConfigurationSection`. Both methods throw `InvalidSetupException` if `T` is not marked with `ConfigSectionNameAttribute`.

### Environment-aware section selection
`ConfigureAll` supports loading a nested sub-section based on an environment key. The environment is read from the key defined in `ConfigurationSections.Environment` (default: `"Environment"`). A per-section `Environment` overrides the top-level one; an empty per-section value disables environment nesting for that section.

```json
{
    "Environment": "Dev1",
    "SectionA": {
        "Environment": "Dev2",
        "Dev1": { "MyValue": "Ignored" },
        "Dev2": { "MyValue": "Used" },
        "MyValue": "Ignored"
    },
    "SectionB": {
        "Dev1": { "MyValue": "Used" },
        "Dev2": { "MyValue": "Ignored" },
        "MyValue": "Ignored"
    },
    "SectionC": {
        "Environment": "",
        "Dev1": { "MyValue": "Ignored" },
        "Dev2": { "MyValue": "Ignored" },
        "MyValue": "Used"
    }
}
```

- `SectionA` defines its own `Environment` (`Dev2`), so its `Dev2` sub-section wins.
- `SectionB` has no override, so the top-level environment (`Dev1`) selects its `Dev1` sub-section.
- `SectionC` sets `Environment` to an empty string, opting out of nesting, so its root values are used.

The environment key should only be overridden at app startup to avoid forcing the use of environment sub-groups everywhere:

```cs
ConfigurationSections.Environment = "Environment";
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — provides the `ITypeCache` and `AddTypeCache` used to discover configuration section types.
