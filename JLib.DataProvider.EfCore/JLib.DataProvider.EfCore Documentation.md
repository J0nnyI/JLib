# JLib.DataProvider.EfCore

Entity Framework Core implementation of the JLib DataProvider abstractions. It provides read and read/write data providers that resolve a `DbContext` via dependency injection and apply JLib authorization filters, along with EF Core query extensions.

## Installation
```sh
dotnet add package JLib.DataProvider.EfCore
```

## Features
- `EfCoreDataProviderR<TEntity>` — a read-only `ISourceDataProviderR<TEntity>` that queries the entities of a `DbContext` resolved from dependency injection (queries are returned `AsNoTracking`).
- `EfCoreDataProviderRw<TEntity>` — a read/write `ISourceDataProviderRw<TEntity>` that supports `Add` and `Remove` (single, batch, and by id) on top of the same `DbContext`.
- Automatic application of JLib authorization: every `Get()` applies the `IAuthorizationInfo<TEntity>.Expression()` filter, and write operations call `AndRaiseException(...)` to enforce authorization before modifying the set.
- `DataObjectQueryableExtensions.ToDictionaryAsync` — turns an `IQueryable<TValue>` of `IDataObject` into an `IReadOnlyDictionary<Guid, TValue>` keyed by `IDataObject.Id`.

## Usage

### Registering the EF Core data providers
The providers resolve a `DbContext` and an `IAuthorizationInfo<TEntity>` from the service provider. They are registered through the `AddDataProvider` extension from `JLib.DataProvider`, supplying one of the EF Core implementations as the provider type. The implementation's generic type argument is resolved per `EntityType` discovered by the type cache.

```cs
using JLib.DataProvider;
using JLib.DataProvider.EfCore;
using JLib.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// a DbContext must be registered so the providers can resolve it
services.AddDbContext<DbContext, MyDbContext>();

services.AddDataProvider<EntityType, EfCoreDataProviderRw<IEntity>, IEntity>(
    typeCache,
    filter: null,
    forceReadOnly: null,
    implementationTypeArgumentResolver: null,
    exceptions,
    loggerFactory);
```

Use `EfCoreDataProviderR<IEntity>` instead of the `Rw` variant (or pass a `forceReadOnly` predicate) when only read access is required.

### Querying entities
Once registered, inject `IDataProviderR<TEntity>` / `IDataProviderRw<TEntity>` (or the `ISourceDataProvider*` aliases) and call `Get()`. The returned `IQueryable<TEntity>` already has the authorization filter applied.

```cs
public class CustomerService
{
    private readonly IDataProviderR<CustomerEntity> _customers;

    public CustomerService(IDataProviderR<CustomerEntity> customers)
        => _customers = customers;

    public Task<List<CustomerEntity>> GetActiveAsync(CancellationToken ct)
        => _customers.Get()
            .Where(c => c.IsActive)
            .ToListAsync(ct);
}
```

### Adding and removing entities
`EfCoreDataProviderRw<TEntity>` exposes the `IDataProviderRw<TEntity>` write operations. Each write is authorized via `IAuthorizationInfo<TEntity>` before the change is staged on the `DbContext`. Persisting the changes (`SaveChanges`) is left to the surrounding unit of work / `DbContext` lifetime.

```cs
public class CustomerWriter
{
    private readonly IDataProviderRw<CustomerEntity> _customers;

    public CustomerWriter(IDataProviderRw<CustomerEntity> customers)
        => _customers = customers;

    public void Create(CustomerEntity customer) => _customers.Add(customer);

    public void Delete(Guid customerId) => _customers.Remove(customerId);
}
```

### Materializing a query into a dictionary by id
`ToDictionaryAsync` builds an `IReadOnlyDictionary<Guid, TValue>` keyed by the `IDataObject.Id`, avoiding the need to repeat the key selector.

```cs
using JLib.DataProvider.EfCore;

IReadOnlyDictionary<Guid, CustomerEntity> byId =
    await _customers.Get().ToDictionaryAsync(cancellationToken);
```

## Related Packages
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md) — the data provider abstractions (`IDataProviderR`, `IDataProviderRw`, `IEntity`, authorization) that this package implements.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — provides the `ITypeCache` used to discover entity types during registration.
