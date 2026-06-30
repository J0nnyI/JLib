# JLib

Metapackage that bundles all JLib packages, including reflection, dependency injection, value types, CQRS, data providers, configuration, and integrations for AutoMapper, EF Core, and HotChocolate. Reference this single package to pull in the complete JLib library.

## Installation
```sh
dotnet add package JLib
```

## Features
- Single entry point that transitively references every JLib package.
- Reflection-driven type discovery and categorization (`ITypeCache`).
- Convention-based dependency injection built on the reflection layer.
- Strongly-typed value types with validation.
- CQRS building blocks (commands, queries, handlers).
- Data provider abstractions with EF Core, in-memory, and AutoMapper-backed implementations.
- Strongly-typed configuration loading.
- Integrations for AutoMapper, EF Core, and HotChocolate (GraphQL).

## Usage
This package contains no source of its own. Adding it pulls in all JLib packages so you can selectively use whichever APIs you need. If you only need a subset, reference the individual packages instead to keep your dependency graph minimal.

```sh
# pull in everything
dotnet add package JLib

# or reference only what you use
dotnet add package JLib.Reflection
dotnet add package JLib.ValueTypes
```

## Related Packages

### Reflection & Dependency Injection
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md)
- [JLib.Reflection.DependencyInjection](../JLib.Reflection.DependencyInjection/JLib.Reflection.DependencyInjection%20Documentation.md)
- [JLib.DependencyInjection](../JLib.DependencyInjection/JLib.DependencyInjection%20Documentation.md)

### Value Types
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md)
- [JLib.ValueTypes.AutoMapper](../JLib.ValueTypes.AutoMapper/JLib.ValueTypes.AutoMapper%20Documentation.md)

### CQRS
- [JLib.Cqrs](../JLib.Cqrs/JLib.Cqrs%20Documentation.md)

### Data Providers
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md)
- [JLib.DataProvider.EfCore](../JLib.DataProvider.EfCore/JLib.DataProvider.EfCore%20Documentation.md)
- [JLib.DataProvider.InMemory](../JLib.DataProvider.InMemory/JLib.DataProvider.InMemory%20Documentation.md)
- [JLib.DataProvider.AutoMapper](../JLib.DataProvider.AutoMapper/JLib.DataProvider.AutoMapper%20Documentation.md)
- [JLib.EfCore](../JLib.EfCore/JLib.EfCore%20Documentation.md)

### Data Generation
- [JLib.DataGeneration](../JLib.DataGeneration/JLib.DataGeneration%20Documentation.md)
- [JLib.DataGeneration.Abstractions](../JLib.DataGeneration.Abstractions/JLib.DataGeneration.Abstractions%20Documentation.md)

### Configuration & Serialization
- [JLib.Configuration](../JLib.Configuration/JLib.Configuration%20Documentation.md)
- [JLib.SystemTextJson](../JLib.SystemTextJson/JLib.SystemTextJson%20Documentation.md)

### Integrations
- [JLib.AutoMapper](../JLib.AutoMapper/JLib.Automapper%20Documentation.md)
- [JLib.HotChocolate](../JLib.HotChocolate/JLib.HotChocolate%20Documentation.md)
- [JLib.Reflection.HotChocolate.Query](../JLib.Reflection.HotChocolate.Query/JLib.Reflection.HotChocolate.Query%20Documentation.md)

### Foundation
- [JLib.Helper](../JLib.Helper/JLib.Helper%20Documentation.md)
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md)
