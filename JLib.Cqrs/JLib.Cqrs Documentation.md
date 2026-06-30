# JLib.Cqrs

Provides interfaces, reflection-discovered types and helpers for building CQRS architectures with JLib. It includes marker interfaces for command entities and typed-id data objects, a persistence accessor abstraction and queryable extensions, built on [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) and [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md).

## Installation
```sh
dotnet add package JLib.Cqrs
```

## Features
- Marker interfaces that classify the command side of a CQRS application:
  - `ICommandEntity` &mdash; flags an entity as the primary domain representation (using value types to ensure data validity).
  - `ITypedIdDataObject<TId>` &mdash; flags a data object whose id is a strongly typed `GuidValueType`.
- `CommandEntityType` &mdash; a reflection `TypeValueType` automatically discovered for every non-abstract class implementing `ICommandEntity`, integrating command entities into JLib's reflection pipeline.
- `IPersistenceAccessor` &mdash; a small abstraction exposing `SaveChanges()` for committing pending changes.
- `QueryableByTypedIdExtensions.ById<T, TId>` &mdash; a type-safe `IQueryable` lookup by a typed (value-type) id, building on `JLib.DataProvider`'s `ById<T>(Guid)` helper.

## Usage

### Marking command entities with typed ids
Define a strongly typed id (a `GuidValueType` from `JLib.ValueTypes`) and an entity that implements both `ICommandEntity` and `ITypedIdDataObject<TId>`. Implementing `ICommandEntity` causes the entity to be discovered as a `CommandEntityType` by JLib's reflection, while `ITypedIdDataObject<TId>` enables the typed `ById` extension.

```cs
using JLib.Cqrs;
using JLib.DataProvider;
using JLib.ValueTypes;

// strongly typed id backed by a Guid
public record OrderId(Guid Value) : GuidValueType(Value);

// command-side domain entity
public class OrderEntity : ICommandEntity, ITypedIdDataObject<OrderId>
{
    // IDataObject requires a Guid Id
    public Guid Id { get; init; } = Guid.NewGuid();

    public OrderId OrderId => new(Id);
}
```

### Querying by a typed id
`ById<T, TId>` lets you resolve a single data object from an `IQueryable<T>` using its strongly typed id instead of a raw `Guid`. It unwraps the value type and delegates to `JLib.DataProvider`'s `ById<T>(Guid)` helper (which performs a `Single` lookup on `Id`).

```cs
using JLib.Cqrs;
using JLib.DataProvider;

// queryable obtained from a data provider, repository or DbSet
IQueryable<OrderEntity> orders = dataProvider.Get();

OrderId id = new(someGuid);

// type-safe lookup by the typed id
OrderEntity order = orders.ById(id);
```

### Committing changes via the persistence accessor
`IPersistenceAccessor` provides a minimal, provider-agnostic way to persist pending changes. Inject it where you need to flush a unit of work.

```cs
using JLib.Cqrs;

public class CreateOrderHandler
{
    private readonly IDataProviderRw<OrderEntity> _orders;
    private readonly IPersistenceAccessor _persistence;

    public CreateOrderHandler(IDataProviderRw<OrderEntity> orders, IPersistenceAccessor persistence)
    {
        _orders = orders;
        _persistence = persistence;
    }

    public void Handle(OrderEntity order)
    {
        _orders.Add(order);
        _persistence.SaveChanges();
    }
}
```

### Discovering command entities via reflection
Because `CommandEntityType` is annotated for JLib's reflection (`Implements(typeof(ICommandEntity))`, `IsClass`, `NotAbstract`), every concrete `ICommandEntity` is materialized as a `CommandEntityType` in the `ITypeCache`. This lets you enumerate all command entities of the application for registration, mapping or validation purposes.

```cs
using JLib.Cqrs;
using JLib.Reflection;

// typeCache built via JLib.Reflection
IEnumerable<CommandEntityType> commandEntities = typeCache.All<CommandEntityType>();
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) &mdash; provides the `TypeValueType` / `ITypeCache` infrastructure that discovers `CommandEntityType`.
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md) &mdash; provides `IDataObject`, `IEntity`, the data provider interfaces and the underlying `ById<T>(Guid)` queryable helper.
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) &mdash; provides `GuidValueType`, the basis for strongly typed ids used by `ITypedIdDataObject<TId>`.
