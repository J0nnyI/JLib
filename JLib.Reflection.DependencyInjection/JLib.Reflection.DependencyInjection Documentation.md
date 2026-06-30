# JLib.Reflection.DependencyInjection

Extension methods to register the [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) type cache (`ITypeCache`) with `Microsoft.Extensions.DependencyInjection`. Provides `AddTypeCache` overloads that build a type package, initialize the cache, and register it as a singleton on the `IServiceCollection`.

## Installation
```sh
dotnet add package JLib.Reflection.DependencyInjection
```

## Features
- Registers the `ITypeCache` as a singleton on an `IServiceCollection`.
- Builds and initializes the type cache during registration and returns the ready-to-use instance via an `out` parameter, so it can be used while configuring further services.
- Multiple overloads: register from an already built `ITypePackage`, or discover assemblies from a directory using included prefixes and a `SearchOption`.
- Collects initialization errors into a supplied `ExceptionBuilder` and uses an `ILoggerFactory` for diagnostics.

## Usage

All overloads live on `ReflectionServiceCollectionExtensions` and are invoked as fluent extension methods on `IServiceCollection`. They expose the initialized `ITypeCache` through an `out` parameter.

### Register from an explicit type package
The most common scenario: build an `ITypePackage` with `TypePackageBuilder`, then register the cache. The returned `typeCache` can immediately be passed to other configuration steps (for example `AddServicesWithAttributes`).

```cs
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var exceptions = new ExceptionBuilder("startup");
var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

var typePackage = new TypePackageBuilder(loggerFactory)
    .AddNestedTypes<MyTypeRoot>()
    .Build();

var services = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory, typePackage);

// typeCache is initialized and registered as a singleton; use it right away
exceptions.ThrowIfNotEmpty();
```

### Discover assemblies from a directory
This overload builds the type package for you by scanning a directory for assemblies matching the given prefixes.

```cs
var services = new ServiceCollection()
    .AddTypeCache(
        out var typeCache,
        exceptions,
        loggerFactory,
        assemblySearchDirectory: AppContext.BaseDirectory,
        searchOption: SearchOption.TopDirectoryOnly,
        includedPrefixes: "MyCompany.MyApp");
```

A convenience overload defaults the search directory to the current directory and uses `SearchOption.TopDirectoryOnly`:

```cs
var services = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory, "MyCompany.MyApp");
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) - provides `ITypeCache`, `TypeCache`, `ITypePackage` and `TypePackageBuilder` that this package registers.
