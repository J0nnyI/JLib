# JLib.ValueTypes.AutoMapper

Provides an AutoMapper `Profile` (`ValueTypeProfile`) that automatically generates two-way mappings between JLib `ValueType` types and their underlying native types. Value types are discovered through the JLib reflection type cache, so no per-type mapping configuration is required.

## Installation
```sh
dotnet add package JLib.ValueTypes.AutoMapper
```

## Features
- Automatically creates AutoMapper maps between every `ValueType<T>` and its native type `T` in both directions.
- Generates nullable variants of those maps as well (e.g. `CustomerId?` <-> `Guid?`).
- Handles both class-based native types (`where TNative : class`) and struct-based native types (`where TNative : struct`).
- Discovers value types automatically via the JLib reflection `ITypeCache`, so no manual `CreateMap` calls are needed.
- Respects opt-out: types marked with `[DisableAutoProfile]` (any `IDisableAutoProfileAttribute`) are skipped, and abstract value types are ignored.
- Collects per-type failures into an `ExceptionBuilder` so a single broken value type does not silently break the whole profile.

## Usage

### Registering the profile
`ValueTypeProfile` is a regular AutoMapper `Profile` that takes an `ITypeCache` and an `ILogger<ValueTypeProfile>` via its constructor. The simplest way to include it is to let the JLib `AddProfiles` helper (from `JLib.AutoMapper`) discover all profiles in the type cache, which includes `ValueTypeProfile`.

```cs
using JLib.AutoMapper;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory,
        TypePackage.GetNested<MyRootType>())
    .AddAutoMapper(cfg => cfg.AddProfiles(typeCache, loggerFactory));
```

### Mapping value types
Given a value type such as:

```cs
public record CustomerId(Guid Value) : GuidValueType(Value);
```

the profile registers maps in both directions, so the mapper can convert between the value type and its native `Guid` automatically:

```cs
var mapper = serviceProvider.GetRequiredService<IMapper>();

// native -> value type
CustomerId id = mapper.Map<CustomerId>(Guid.NewGuid());

// value type -> native
Guid raw = mapper.Map<Guid>(id);

// nullable variants are also mapped
CustomerId? maybeId = mapper.Map<CustomerId?>((Guid?)null); // -> null
```

Value-type-to-native conversions use the value's `Value` property; native-to-value conversions are produced through `ValueType.FactoryExpressions`, which run the value type's validation when constructing the instance.

### Opting a value type out of automatic mapping
Apply `[DisableAutoProfile]` (or any attribute implementing `IDisableAutoProfileAttribute`) to a value type to exclude it from the generated maps:

```cs
using JLib.Reflection;

[DisableAutoProfile]
public record InternalToken(string Value) : StringValueType(Value);
```

## Related Packages
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) - the value type base classes (`ValueType<T>`, `GuidValueType`, `StringValueType`, ...) this package maps.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) - provides the `ITypeCache`, `ValueTypeType` and `DisableAutoProfileAttribute` used for value type discovery and opt-out.
- [JLib.AutoMapper](../JLib.AutoMapper/JLib.Automapper%20Documentation.md) - provides the `AddProfiles` helper that discovers and registers this profile from the type cache.
