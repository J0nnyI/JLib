# JLib.DataProvider.AutoMapper

Provides read data providers for the JLib DataProvider abstraction that project one data object onto another using AutoMapper. Maps are generated automatically for each mapped data object type, so a mapped provider can be injected directly without writing manual mapping code.

## Installation
```sh
dotnet add package JLib.DataProvider.AutoMapper
```

## Features
- Generates an AutoMapper map for every mapped data object type via the `MappedDataObjectProfile`, driven by the JLib reflection `ITypeCache`.
- Registers read data providers for each `IMappedDataObjectType` so the mapped type can be injected as an `IDataProviderR<T>` / `ISourceDataProviderR<T>` without manual mapping.
- Uses AutoMapper queryable projection (`ProjectTo`) so mapping happens inside the underlying provider's `IQueryable`, keeping the projection composable and provider-side where supported.
- Honors existing repositories: if a hand-written `IDataProviderR<T>` repository already provides the destination type, only the source provider alias is registered; conflicting read/write setups are reported as setup exceptions.

## Usage

### Defining a mapped data object
A mapped type is recognized through a type value type that implements `IMappedDataObjectType`. Each entry in `MappingInfo` declares a source type, a destination type and a `MappingDataProviderMode` (`Disabled` or `Read`). The destination type itself is an `IDataObject` that exposes the properties to be projected from the source.

```cs
using JLib.DataProvider;

public class PersonEntity : IEntity
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
}

// the mapped, read-only view that is projected from PersonEntity
public class PersonGdo : IDataObject
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
}
```

Properties can be marked required for mapping by adding the `[Required]` attribute (`System.ComponentModel.DataAnnotations`) or by using the `required` keyword on .NET 7 or higher.

### Registering the map data providers
`AddMapDataProvider` walks every `IMappedDataObjectType` in the `ITypeCache` and registers a `MapDataProviderR<TFrom, TTo>` for each mapping entry whose mode is `Read`. The mapped destination type can then be injected directly.

```cs
using JLib.DataProvider.AutoMapper;
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

var exceptions = new ExceptionBuilder("Setup");

services.AddMapDataProvider(typeCache, exceptions);

exceptions.ThrowIfNotEmpty();
```

For each `Read` mapping, `AddMapDataProvider` registers `MapDataProviderR<TFrom, TTo>` as `ISourceDataProviderR<TTo>`. When no repository already provides the destination type, it is additionally registered as the plain `IDataProviderR<TTo>`, so it can be injected directly. If a repository for the destination type exists but is writable while the mapping is `Read`, a setup exception is added describing the mismatch.

### Generating the AutoMapper profile
`MappedDataObjectProfile` creates an AutoMapper map for every mapping entry of every `IMappedDataObjectType` (skipping types where automated profile generation is disabled). Register it like any other AutoMapper `Profile`, supplying the `ITypeCache`:

```cs
using AutoMapper;
using JLib.DataProvider.AutoMapper;
using JLib.Reflection;

var mapperConfig = new MapperConfiguration(cfg =>
    cfg.AddProfile(new MappedDataObjectProfile(typeCache, logger)));

IMapper mapper = mapperConfig.CreateMapper();
```

### Consuming a mapped provider
Once registered, inject the destination type's data provider. The provider reads from the source type's `IDataProviderR<TFrom>` and projects the result to the destination type using AutoMapper:

```cs
public class PersonService
{
    private readonly IDataProviderR<PersonGdo> _people;

    public PersonService(IDataProviderR<PersonGdo> people)
        => _people = people;

    public IQueryable<PersonGdo> All() => _people.Get();
}
```

Internally the registered `MapDataProviderR<PersonEntity, PersonGdo>` resolves the underlying `IDataProviderR<PersonEntity>` and returns `provider.Get().ProjectTo<PersonGdo>(config)`.

## Related Packages
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md) - the data provider abstraction (`IDataObject`, `IDataProviderR<T>`, `ISourceDataProviderR<T>`, repositories) that this package plugs into.
- [JLib.AutoMapper](../JLib.AutoMapper/JLib.Automapper%20Documentation.md) - AutoMapper helpers and profiles used alongside the generated maps.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) - provides the `ITypeCache` and type value types that drive provider and profile generation.
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) - provides the `ExceptionBuilder` used to collect setup errors during registration.
