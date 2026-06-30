# JLib.AutoMapper

Integrates [AutoMapper](https://automapper.org/) with the JLib reflection type cache to automatically discover and register all AutoMapper `Profile` types, injecting `ITypeCache` and `ILoggerFactory` into their constructors. It also provides helper profiles for mapping objects to string dictionaries and nullable struct conversion, plus a small mapping extension method.

## Installation
```sh
dotnet add package JLib.AutoMapper
```

## Features
- Automatic discovery and registration of all `AutoMapper.Profile` types via the JLib reflection type cache (`AddProfiles` extension on `IMapperConfigurationExpression`).
- Dependency injection into profile constructors: profiles may declare `ITypeCache`, `ILoggerFactory`, or `ILogger<TProfile>` parameters, which are resolved automatically during instantiation.
- `TypeToDictionaryProfile<T>` — a generic profile that maps any type `T` to a `Dictionary<string, string?>`, including all public properties (and fields) even when their value is null.
- `StructNullabilityMapper` — a ready-made profile mapping `bool?` to `bool` (null becomes `false`).
- `MapTo<T>` — a fluent extension method to map any object to `T` using a given `IMapper`.

## Usage

### Auto-registering all profiles from the type cache
Once a JLib `ITypeCache` is available, `AddProfiles` finds every non-abstract, non-generic `Profile` (identified by the `AutoMapperProfileType`) and adds it to the AutoMapper configuration. Each profile is instantiated through the cache, so it can request `ITypeCache`, `ILoggerFactory`, or `ILogger<TProfile>` in its constructor.

```cs
using AutoMapper;
using JLib.AutoMapper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory /* , type packages */)
    .AddLogging()
    .AddAutoMapper(cfg => cfg.AddProfiles(typeCache, loggerFactory));
```

### Writing a profile that uses injected dependencies
A discovered `Profile` may receive the type cache and/or a logger through its constructor.

```cs
using AutoMapper;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

public class MyProfile : Profile
{
    public MyProfile(ITypeCache typeCache, ILogger<MyProfile> logger)
    {
        logger.LogDebug("configuring MyProfile");
        CreateMap<Source, Destination>();
    }
}
```

### Mapping an object to a string dictionary
`TypeToDictionaryProfile<T>` adds a map from `T` to `Dictionary<string, string?>`. All public properties (and fields) are emitted, with non-string values converted via `ToString()` and null values preserved.

```cs
using AutoMapper;
using JLib.AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

class Demo
{
    public string Name { get; set; } = "initial";
    public int Value { get; set; } = 2;
    public string? Null { get; set; } = null;
}

var mapper = new MapperConfiguration(
        x => x.AddProfile<TypeToDictionaryProfile<Demo>>(),
        NullLoggerFactory.Instance)
    .CreateMapper();

Dictionary<string, string?> result = mapper.Map<Dictionary<string, string?>>(new Demo());
// { "Name": "initial", "Value": "2", "Null": null }
```

### Converting nullable bool to bool
Add `StructNullabilityMapper` to map `bool?` to `bool`, treating `null` as `false`.

```cs
cfg.AddProfile<StructNullabilityMapper>();
```

### Fluent mapping with MapTo
```cs
using JLib.AutoMapper;

Destination dest = source.MapTo<Destination>(mapper);
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — provides the `ITypeCache` and `TypeValueType` infrastructure used to discover profiles.
- [JLib.DataGeneration](../JLib.DataGeneration/JLib.DataGeneration%20Documentation.md) — uses this package's `AddProfiles` to wire up AutoMapper in its setup.
