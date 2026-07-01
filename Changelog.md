# 0.13.0
## Breaking Changes
### JLib.Reflection
- removed all typePackage usages and implementations. Use the TypePackageBuilder instead.
  - calls will be cached by default. 

### JLib.Helper
- changed ReflectionHelper attribute method declarations to cache the result.

### JLib.Helper
- changed ReflectionHelper attribute method declarations to cache the result.

### JLib.ValueTypes.*
- renamed JLib.ValueTypes.Mapping -> JLib.ValueTypes.AutoMapper
- moved valueType json converters to JLib.ValueTypes package

### JLib.DataGeneration / JLib.DataGeneration.Abstractions
- removed the AutoMapper dependency. `TestingIdGenerator` no longer takes an `IMapper` constructor parameter and creates typed value-type ids via `ValueType.Create` instead of AutoMapper. Consumers no longer need to call `.AddAutoMapper(...)` for id generation.

### JLib.DependencyInjection
- removed the transitive `JLib.AutoMapper` project reference (it was unused by the package itself). Projects that obtained AutoMapper transitively via `JLib.DependencyInjection` must now reference `JLib.AutoMapper` and/or the `AutoMapper` package explicitly.

### JLib.Configuration
- removed the multi-environment config section feature. Config sections are now loaded directly under their `ConfigSectionNameAttribute` key; the `Environment` key and the nested per-environment subsections are no longer evaluated by `AddAllConfigSections` and `GetSection`/`GetSectionObject`. The `ConfigurationSections` class was removed. Use the standard .NET configuration providers and layer environment specific config section files (e.g. `appsettings.{Environment}.json`) when building the `IConfiguration` instead.
- removed the `GetSection<T>(this IConfiguration, string configSectionName, ILoggerFactory)` overload. It only wrapped `IConfiguration.GetSection(string)` after the environment feature was dropped. Use the attribute based `GetSection<T>(this IConfiguration, ILoggerFactory)` overload or the native `IConfiguration.GetSection` directly.
- `AddAllConfigSections` no longer registers a directly injectable instance of each config section, and the `ServiceLifetime lifetime` parameter was removed. Sections are now only bound via `Configure<T>` and must be consumed through `IOptions<T>`, `IOptionsSnapshot<T>` or `IOptionsMonitor<T>`. This restores control over the update/reload semantics to the consuming code; the previous direct instance was always the frozen `IOptions<T>.Value` regardless of the requested lifetime. Replace `GetRequiredService<MySection>()` with `GetRequiredService<IOptions<MySection>>().Value`.
- `AddAllConfigSections` no longer takes an `ILoggerFactory` parameter. The new signature is `AddAllConfigSections(this IServiceCollection, ITypeCache, IConfiguration)`.

## Features
### JLib.Helper
- AttributeCache added
### JLib.Configuration
- added `AddConfigSection<T>(this IServiceCollection, IConfiguration)`: a strongly typed, AOT friendly registration that binds a single config section via `Configure<T>`, resolving the section name from the type's `ConfigSectionNameAttribute`.
- added the non-generic `AddConfigSection(this IServiceCollection, IConfiguration, Type)` for section types only known at runtime. `AddAllConfigSections` now delegates to it, so the runtime reflection lives in a single named place.
- added `ConfigSectionNameAttribute.ResolveSectionName(Type)` which resolves the declared `ConfigSectionName` (or throws `InvalidSetupException`).
- `ConfigurationSectionType` now validates that a config section is not an open generic type (`IValidatedType` / `ShouldNotBeGeneric`). A `[ConfigSectionName]` on a generic class is reported as a type cache build error (naming the offending type) instead of silently doing nothing or crashing later in `AddAllConfigSections` with an opaque reflection error.
### JLib.AspNetCore
- added an `IHostApplicationBuilder.AddAllConfigSections(ITypeCache)` overload that binds all discovered config sections against `builder.Configuration` and registers them into `builder.Services`.

# 0.12.0
## Breaking Changes
### Package References
- Updated all references, including automapper, which causes an indirect breaking change.
## Features
### DotNet
- Added support for .NET 10
### JLib.Reflection
#### Type Package
- TypePackageBuilder added
    - Significantly improved performance over the previous implementation
- TypePackage Deprectaed
    - The Builder should be used instead
### JLib.Configuration
#### Environment
- The Environment key can now be changed, to be able to not use it. The default value is set by vs per default.

## Improvements
### Jlib.Reflection
#### Typecache
- added details to thrown exceptions
### JLib.DataGeneration
- AutoMapper References removed


