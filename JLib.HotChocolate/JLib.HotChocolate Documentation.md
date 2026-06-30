# JLib.HotChocolate

HotChocolate GraphQL integration for JLib. It provides type-cache driven discovery of GraphQL type extensions and query data object types, resolver context helpers for batched data loading of JLib data objects via EF Core, and a logging diagnostics event listener.

## Installation
```sh
dotnet add package JLib.HotChocolate
```

## Features
- Discovers GraphQL type extensions from the JLib `ITypeCache` and registers them on the schema in one call (`AddTypeExtensions`).
- Flag interfaces (`IQueryDataObject`, `IGraphQlDataObject`) that mark JLib data objects as GraphQL query types, recognized by the reflection type cache as `QueryDataObjectType`.
- Validation of query data object types (constructor / initialization rules) via the JLib reflection validation pipeline.
- Resolver context helpers that batch-load JLib data objects through `IDataProviderR<T>` and EF Core (`GetOneDataObjectAsync`, `GetManyDataObjectsAsync`).
- A `LoggingEventListener` that logs request, operation, subscription, and error diagnostics events of the HotChocolate execution engine.

## Usage

### Registering type extensions from the type cache
`RequestExecutorBuilderHelper.AddTypeExtensions` walks the JLib `ITypeCache` for every discovered `TypeExtensionType` and registers it as a HotChocolate type extension. A `TypeExtensionType` is any class annotated with `[ExtendObjectType]` / `[ExtendObjectType<T>]` or derived from `ObjectTypeExtension`.

> Note: this method must be called **after all** JLib DataProviders have been registered.

```cs
using JLib.HotChocolate.Helper;
using JLib.Reflection;

ITypeCache typeCache = /* the JLib type cache */;

builder.Services
    .AddGraphQLServer()
    .AddQueryType()
    .AddTypeExtensions(typeCache);
```

```cs
using HotChocolate.Types;

[ExtendObjectType<Query>]
public class CustomerQueries
{
    public IQueryable<CustomerGdo> GetCustomers([Service] IDataProviderR<CustomerGdo> provider)
        => provider.Get();
}
```

### Marking data objects as GraphQL query types
Implement `IQueryDataObject` (or `IGraphQlDataObject`) on a JLib data object so the type cache recognizes it as a `QueryDataObjectType`. The accompanying validation enforces that such types are constructible by HotChocolate (a single non-public/parameterless constructor, or no public parameterless constructor when non-nullable properties must be initialized).

```cs
using JLib.HotChocolate;

public record CustomerGdo(Guid Id, string Name) : IGraphQlDataObject;
```

### Batched data loading in resolvers
`ResolverContextHelper` provides extension methods on `IResolverContext` that resolve JLib data objects through the registered `IDataProviderR<T>` using HotChocolate batch data loaders. A new DI scope is created per batch to avoid disposed-context exceptions on circular loader calls. These helpers use EF Core specific methods (`ToDictionaryAsync` / `ToListAsync`).

```cs
using HotChocolate.Resolvers;
using JLib.HotChocolate.Helper;

[ExtendObjectType<OrderGdo>]
public class OrderResolvers
{
    // load a single related data object by its primary key
    public Task<CustomerGdo> GetCustomer([Parent] OrderGdo order, IResolverContext context)
        => context.GetOneDataObjectAsync<CustomerGdo>(order.CustomerId);

    // load many related data objects grouped by a foreign key
    public Task<LineItemGdo[]> GetLineItems([Parent] OrderGdo order, IResolverContext context)
        => context.GetManyDataObjectsAsync<LineItemGdo>(order.Id, li => li.OrderId);
}
```

### Logging execution diagnostics
Register the `LoggingEventListener` to log HotChocolate request, operation, subscription, and error events through `ILogger`.

```cs
using JLib.HotChocolate;

builder.Services
    .AddGraphQLServer()
    .AddDiagnosticEventListener<LoggingEventListener>();
```

## Related Packages
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md) — supplies the `IDataObject` / `IDataProviderR<T>` abstractions used by the data object types and resolver helpers.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — provides the `ITypeCache` and `TypeValueType` / validation infrastructure that drives type extension and query data object discovery.
