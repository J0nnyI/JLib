# JLib.DataProvider

Provides data access abstractions for JLib, including generic read and read-write data provider and repository interfaces keyed by `Guid` ids, an in-memory implementation, and a data object authorization model. It uses the JLib reflection type cache to automatically register data providers and repositories with the dependency injection container.

## Installation

```sh
dotnet add package JLib.DataProvider
```

## Features

- Generic, repository-style data access interfaces keyed by `Guid` id: `IDataProviderR<TDataObject>` (read) and `IDataProviderRw<TDataObject>` (read-write).
- A clear separation between the consumer-facing data providers and the underlying `ISourceDataProviderR<TDataObject>` / `ISourceDataProviderRw<TDataObject>` data sources, so custom repositories can wrap a source data provider.
- Flag interfaces `IDataObject` (has an `Id`) and `IEntity` (read-write capable) plus the reflection type value types `EntityType`, `RepositoryType`, `SourceDataProviderType` used for automated discovery.
- Automatic registration with the `IServiceCollection` via `AddDataProvider<...>` and `AddRepositories`, including read/write mode validation and detection of conflicting repositories.
- An in-memory implementation (`InMemoryDataProvider<TEntity>`) intended primarily for testing.
- A data object authorization model: derive from `AuthorizationProfile` to declare per-`IDataObject` rules that filter queries and validate materialized objects, registered via `AddDataAuthorization`.

## Usage

### Defining a data object

A data object exposes a `Guid` `Id`. Implement `IEntity` (which derives from `IDataObject`) to make it editable through a read-write data provider. In practice the concrete entity flag interface comes from a consuming package (for example `ICommandEntity` from `JLib.Cqrs`).

```cs
using JLib.DataProvider;

public class Customer : IEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = "";
}
```

### Registering data providers

`AddDataProvider<TTvt, TImplementation, TIgnoredDataObject>` scans the type cache for all data object types matching `TTvt` and registers the given implementation, wiring up `IDataProviderR`, `IDataProviderRw`, `ISourceDataProviderR` and `ISourceDataProviderRw` aliases as appropriate. The example below uses the built-in `InMemoryDataProvider<IEntity>` for command entities.

```cs
using JLib.DataProvider;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

services.AddDataProvider<CommandEntityType, InMemoryDataProvider<IEntity>, IEntity>(
    typeCache,
    filter: null,            // null => all matching data object types
    forceReadOnly: null,     // null => keep the implementation's read/write capability
    implementationTypeArgumentResolver: null, // null => use the data object type as the single generic argument
    exceptions,
    loggerFactory);
```

The `filter` and `forceReadOnly` delegates let you restrict registration to specific data object types or expose an otherwise writable provider as read-only:

```cs
// only register a provider for one entity type
services.AddDataProvider<CommandEntityType, InMemoryDataProvider<IEntity>, IEntity>(
    typeCache, tvt => tvt.Value == typeof(Customer), null, null, exceptions, loggerFactory);

// force read-only access even though the implementation can write
services.AddDataProvider<CommandEntityType, InMemoryDataProvider<IEntity>, IEntity>(
    typeCache, null, _ => true, null, exceptions, loggerFactory);
```

### Custom repositories

A repository is a non-generic class implementing `IDataProviderR<TDataObject>` (or `IDataProviderRw<TDataObject>`) for one specific data object. It typically wraps the registered `ISourceDataProviderR<TDataObject>` / `ISourceDataProviderRw<TDataObject>`. Call `AddRepositories` to register all discovered repositories; it fails the setup if two repositories provide the same data object, and the data provider registration verifies that the repository's read/write mode matches the source.

```cs
using JLib.DataProvider;

public class CustomerRepository : DataProviderRBase<Customer>, IDataProviderR<Customer>
{
    private readonly ISourceDataProviderR<Customer> _source;
    public CustomerRepository(ISourceDataProviderR<Customer> source) => _source = source;

    public override IQueryable<Customer> Get() => _source.Get();
}
```

```cs
services.AddRepositories(typeCache, exceptions);
```

### Reading and writing data

Inject the data provider and use the `Guid`-keyed API. Reads return an `IQueryable<TDataObject>` or resolve single/multiple objects by id; writes are available through `IDataProviderRw<TDataObject>`.

```cs
public class CustomerService
{
    private readonly IDataProviderRw<Customer> _customers;
    public CustomerService(IDataProviderRw<Customer> customers) => _customers = customers;

    public void Example()
    {
        _customers.Add(new Customer { Name = "Ada" });

        IQueryable<Customer> all = _customers.Get();
        Customer one = _customers.Get(someId);                 // throws DataObjectNotFoundException if missing
        Customer? maybe = _customers.TryGet(someId);           // null if missing
        bool exists = _customers.Contains(someId);
        IReadOnlyDictionary<Guid, Customer> many = _customers.Get(new[] { id1, id2 });

        _customers.Remove(someId);
    }
}
```

### Authorizing data objects

Derive from `AuthorizationProfile` to declare authorization rules per data object. Each rule provides a query predicate (applied to the `IQueryable`) and a data object predicate (applied to a materialized object). Register the profiles with `AddDataAuthorization`, which also requires a scope provider (`AddScopeProvider`).

```cs
using JLib.DataProvider;
using JLib.DataProvider.Authorization;
using JLib.Reflection;

public class CustomerAuthProfile : AuthorizationProfile
{
    public CustomerAuthProfile(ITypeCache typeCache) : base(typeCache)
    {
        AddAuthorization<Customer, ICurrentUser>(
            user => customer => customer.OwnerId == user.Id, // authorize queries
            (user, customer) => customer.OwnerId == user.Id); // authorize materialized objects
    }
}
```

```cs
services
    .AddScopeProvider()
    .AddDataAuthorization(typeCache);
```

Authorization is transparent to consumers: an authorized data provider filters `Get()` results and throws `DataObjectNotFoundException` when a non-authorized object is requested by id. You can verify that every data object type of a given kind is covered with `AuthorizationProfile.EnsureAuthorised<TDataObjectType>()`.

## Related Packages

- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) - the type cache and type value types that drive automatic discovery and registration.
- [JLib.DependencyInjection](../JLib.DependencyInjection/JLib.DependencyInjection%20Documentation.md) - service registration helpers (aliases, generic services, scope provider) used by the registration extensions.
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) - the `ExceptionBuilder` aggregation model used during setup validation.
- [JLib.Helper](../JLib.Helper/JLib.Helper%20Documentation.md) - reflection and collection helper extensions used throughout.
