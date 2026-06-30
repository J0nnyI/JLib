# JLib.Reflection

Provides a cached, strongly-typed reflection framework for JLib. Types are grouped into `TypeValueType`s that are discovered via factory attributes, validated and navigated at startup through a central `TypeCache`, and bundled using `TypePackage`s so reflection results are computed once and reused.

## Installation
```sh
dotnet add package JLib.Reflection
```

> To register the `ITypeCache` in a `Microsoft.Extensions.DependencyInjection` container, add the companion package
> [JLib.Reflection.DependencyInjection](../JLib.Reflection.DependencyInjection/JLib.Reflection.DependencyInjection%20Documentation.md), which provides the `AddTypeCache` extension method.

## Features
- **Strongly typed reflection**: instead of passing raw `System.Type` values around, you work with domain-specific `TypeValueType`s (e.g. an `EntityType` or `DtoType`), improving readability and type safety.
- **Reflection caching**: all reflection is performed once when the `TypeCache` is built and reused from that point on.
- **Simplified type discovery**: types are classified automatically by decorating your `TypeValueType` with `TvtFactoryAttribute` filter attributes.
- **Start-time validation**: `TypeValueType`s implementing `IValidatedType` are validated when the cache is created, so invalid types fail fast at startup.
- **Navigation properties**: a `NavigatingTypeValueType` can reference other `TypeValueType`s, allowing you to navigate the type graph (e.g. from an entity to its mapped DTO).
- **Composable type packages**: `ITypePackage`s describe which assemblies and types feed the cache and can be merged, filtered and black-listed via the `TypePackageBuilder`.

## Core Components
- **`TypeValueType`** — an abstract `record` deriving from `JLib.ValueTypes.ValueType<Type>`. It wraps a single `System.Type` and is the base of every strongly-typed reflection type. Instances must not be created manually; they are produced by the `TypeCache`.
- **`TvtFactoryAttribute`** — the base class for the filter attributes that decide which `Type`s a `TypeValueType` applies to. A type is assigned to a `TypeValueType` only if **all** of its factory attributes return `true`.
- **`ITypeCache` / `TypeCache`** — discovers, groups, validates and serves `TypeValueType` instances. All reflection happens in the constructor; it should be used as a singleton.
- **`ITypePackage` / `TypePackageBuilder`** — describe and build the set of types that the `TypeCache` operates on.

## Usage

### Defining a TypeValueType
Derive a `record` from `TypeValueType` (or `NavigatingTypeValueType`) and decorate it with one or more `TvtFactoryAttribute`s to define which types it matches.

```cs
using JLib.Reflection;

public interface IEntity { }

// matches every non-abstract class that implements IEntity
[TvtFactoryAttribute.Implements(typeof(IEntity))]
[TvtFactoryAttribute.NotAbstract]
public record EntityType(Type Value) : TypeValueType(Value);
```

Some of the available filter attributes (all nested in `TvtFactoryAttribute`):

| Attribute | Matches types that |
| --- | --- |
| `IsClass` / `IsInterface` | are a class / interface |
| `NotAbstract` / `NotGeneric` / `BeGeneric` | satisfy the corresponding `Type` flag |
| `Implements(Type)` / `ImplementsAny(Type)` / `ImplementsNone(Type)` | implement the given (open) generic interface |
| `IsAssignableTo(Type)` / `IsDerivedFrom(Type)` / `DerivedFromAny(Type)` | are assignable to / derived from the given type |
| `HasAttribute(Type)` / `HasInterfaceWithAttribute(Type)` | carry the given attribute |

On .NET 7 or greater, generic variants such as `Implements<T>`, `IsAssignableTo<T>` and `IsDerivedFrom<T>` are also available.

When several `TypeValueType`s match the same type, decorate them with `[TvtFactoryAttribute.Priority(value)]` (lowest value wins, default is `10_000`); an ambiguous match without a deciding priority is reported as a setup error.

### Building a TypePackage and the TypeCache
The `TypePackageBuilder` decides which assemblies and types the cache sees. `TypeCache` takes the built package, an `ExceptionBuilder` and an `ILoggerFactory`.

```cs
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());

// build the package describing which types to consider
ITypePackage package = new TypePackageBuilder()
    .AddAssemblyOf<EntityType>(AssemblyLoadMode.Recursive)
    .Build();

// all reflection happens here
var exceptions = new ExceptionBuilder("TypeCache setup");
ITypeCache cache = new TypeCache(package, exceptions, loggerFactory);
exceptions.ThrowIfNotEmpty();
```

### Querying the cache
Once built, the cache resolves the strongly-typed instance for any known `Type`.

```cs
// all entity types
IEnumerable<EntityType> entities = cache.All<EntityType>();

// the EntityType for a specific CLR type
EntityType byType = cache.Get<EntityType, MyEntity>();
EntityType byWeakType = cache.Get<EntityType>(typeof(MyEntity));

// the single entity matching a predicate (throws if 0 or more than 1 match)
EntityType single = cache.Get<EntityType>(e => e.Name == "MyEntity");

// non-throwing lookup
EntityType? maybe = cache.TryGet<EntityType, MyEntity>();
```

### Validating types at startup
Implement `IValidatedType` to validate a type when the cache is created. Validation errors are aggregated into the cache's `ExceptionBuilder`, so the application fails fast on an invalid configuration.

```cs
[TvtFactoryAttribute.Implements(typeof(IEntity))]
public record EntityType(Type Value) : TypeValueType(Value), IValidatedType
{
    public void Validate(ITypeCache cache, IValidationContext<Type> value)
    {
        if (Value.GetConstructor(Type.EmptyTypes) is null)
            value.AddSubValidators(
                new Exception($"{Name} must have a parameterless constructor").ToProvider());
    }
}
```

### Navigating between TypeValueTypes
Derive from `NavigatingTypeValueType` and use the protected `Navigate<T>` helper to reference other `TypeValueType`s. The navigation is materialized by the cache after all types have been discovered.

```cs
public record EntityType(Type Value) : NavigatingTypeValueType(Value)
{
    // resolves the DtoType associated with this entity from the cache
    public DtoType Dto => Navigate(cache => cache.Get<DtoType>(/* resolve the dto type */));
}
```

Types that need to run code after navigation is set up (but before validation) can implement `IPostNavigationInitializedType.Initialize`.

### Shaping the package: filters, black-lists and file-system loading
The `TypePackageBuilder` is fluent and offers fine-grained control over its contents.

```cs
ITypePackage package = new TypePackageBuilder()
    // include an assembly and its peer dependencies
    .Add(AssemblyLoadMode.Recursive, typeof(MyEntity).Assembly)
    // include all matching DLLs from the executing directory
    .AddFromPath(directory: null, includedPrefixes: ["MyApp."])
    // include the nested types of a container class (useful for tests)
    .AddNestedTypes<MyTestTypes>()
    // exclude individual assemblies or types
    .AddToBlacklist(typeof(SomeUnwantedType))
    // keep only the types that pass every filter
    .AddTypeFilter(t => t.Namespace?.StartsWith("MyApp") == true)
    .Build();
```

`AssemblyLoadMode` controls how dependencies are pulled in:
- `TopLevelOnly` — only the given assembly is loaded.
- `Recursive` — the assembly and all its peer dependencies are loaded.

Packages can also be combined after the fact via `TypePackageExtensions.Merge`, `MergeWith` and serialized for inspection with `ToJson`.

### Excluding types from the cache
Decorate a type with `[IgnoreInCache]` to keep it out of the discovery process entirely.

## Related Packages
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) — provides the `ValueType<T>` base that `TypeValueType` derives from.
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) — provides the `ExceptionBuilder` used to aggregate setup and validation errors.
- [JLib.Reflection.DependencyInjection](../JLib.Reflection.DependencyInjection/JLib.Reflection.DependencyInjection%20Documentation.md) — registers the `ITypeCache` in a dependency-injection container via `AddTypeCache`.
- [JLib.AutoMapper](../JLib.AutoMapper/JLib.Automapper%20Documentation.md) — builds AutoMapper profiles from `TypeValueType`s discovered by the cache.
