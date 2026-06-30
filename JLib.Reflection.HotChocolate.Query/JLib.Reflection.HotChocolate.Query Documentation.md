# JLib.Reflection.HotChocolate.Query

Adds a [HotChocolate](https://chillicream.com/docs/hotchocolate) GraphQL `ObjectType` that exposes the application's cached reflection state, letting you query the JLib type cache, registered services, and data providers over a GraphQL endpoint.

## Installation
```sh
dotnet add package JLib.Reflection.HotChocolate.Query
```

## Features
- `TypeCacheGdo` &mdash; a GraphQL-ready object exposing the contents of the JLib `ITypeCache`:
  - `All` &mdash; every `TypeValueType` in the cache.
  - `ByTypeValueType` &mdash; the cached types grouped by their `TypeValueType` kind.
  - `ByAssembly` &mdash; the cached types grouped by assembly, then by `TypeValueType` kind.
  - `KnownTypeValueTypes` &mdash; all `TypeValueType` kinds the cache is aware of.
  - `IncludedAssemblies` &mdash; the distinct assemblies that contributed cached types.
- All list-returning fields are annotated with HotChocolate's `[UseFiltering]`, so the exposed reflection data can be filtered directly in the GraphQL query.
- GraphQL-friendly wrapper types ("Gdo" = GraphQL Data Object) for reflection primitives that are not directly serializable:
  - `TypeGdo` wraps `System.Type` (`Name`, `Namespace`, `FullName`, `TypeArguments`, `ImplementedInterfaces`).
  - `TypeValueTypeGdo` / `TypeValueTypeGroupGdo` / `AssemblyGdo` / `AssemblyTypeGroupGdo` for cache entries and groupings.
  - `ServiceInfoGdo` resolves a service type to its registered implementation and the `DataProvider`s it references.
  - `DataProviderInfoGdo` exposes the `IDataProviderR<>` and `ISourceDataProviderR<>` registrations for a `DataObjectType`.
- `IGraphQlReflectionEndpointCache` / `GraphQlReflectionEndpointCache` &mdash; a scoped, de-duplicating cache that turns reflection objects into their Gdo wrappers so each `Type` / service / data provider is materialized only once per request.

## Usage
### Registering the cache service
The Gdo resolvers depend on `IGraphQlReflectionEndpointCache` being available via DI. Register it alongside your HotChocolate GraphQL server. `GraphQlReflectionEndpointCache` resolves the JLib `ITypeCache` from the container, so make sure the type cache has been registered as well (see [JLib.Reflection.DependencyInjection](../JLib.Reflection.DependencyInjection/JLib.Reflection.DependencyInjection%20Documentation.md)).

```cs
using JLib.Reflection.HotChocolate.Query;

builder.Services
    .AddScoped<IGraphQlReflectionEndpointCache, GraphQlReflectionEndpointCache>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddFiltering();
```

### Exposing the reflection state on the query root
Add a `TypeCacheGdo` field to your query type. HotChocolate injects the application's `ITypeCache` into the `TypeCacheGdo` constructor, and the `[Service]`-annotated resolver parameters pull `IGraphQlReflectionEndpointCache` from DI.

```cs
public class Query
{
    public TypeCacheGdo Reflection([Service] ITypeCache typeCache)
        => new(typeCache);
}
```

### Querying the type cache
With the field in place, the cached reflection state can be queried (and filtered) over GraphQL:

```graphql
query {
  reflection {
    includedAssemblies {
      fullName
    }
    byTypeValueType {
      typeValueTypeType { name }
      types {
        name
        namespace
        fullName
      }
    }
    all {
      self { name }
      value { name }
    }
  }
}
```

### Converting reflection objects to Gdo wrappers manually
`IGraphQlReflectionEndpointCache` can also be used directly to wrap a `Type`, service, or `DataObjectType` &mdash; results are cached per instance so repeated conversions return the same Gdo.

```cs
public class MyResolver
{
    public TypeGdo? Wrap(Type type, [Service] IGraphQlReflectionEndpointCache cache)
        => cache.ToGdo(type);

    public ServiceInfoGdo? Service(Type service, [Service] IGraphQlReflectionEndpointCache cache)
        => cache.ToServiceInfoGdo(service);
}
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) &mdash; provides the `ITypeCache` and `TypeValueType` model this package exposes.
- [JLib.Reflection.DependencyInjection](../JLib.Reflection.DependencyInjection/JLib.Reflection.DependencyInjection%20Documentation.md) &mdash; registers the `ITypeCache` in the DI container.
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md) &mdash; supplies the `IDataProviderR<>` / `ISourceDataProviderR<>` / `DataObjectType` model surfaced by `DataProviderInfoGdo`.
- [JLib.HotChocolate](../JLib.HotChocolate/JLib.HotChocolate%20Documentation.md) &mdash; shared HotChocolate helpers and conventions used by this package.
