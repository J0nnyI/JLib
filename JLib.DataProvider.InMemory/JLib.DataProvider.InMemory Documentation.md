# JLib.DataProvider.InMemory

Provides a simple, thread-safe in-memory implementation of the JLib DataProvider abstractions, intended primarily for testing. Data is not persisted between instances, so providers are registered as singletons.

## Installation
```sh
dotnet add package JLib.DataProvider.InMemory
```

## Features
- Registers `IDataProviderR<TData>` and `IDataProviderRw<TDataObject>` backed by the thread-safe `InMemoryDataProvider<TEntity>` implementation for every matching `IDataObjectType`.
- Integrates with the JLib reflection `ITypeCache` so providers are wired up automatically for the discovered entity types.
- Stores all data in a `ConcurrentDictionary`, making it safe to use concurrently in tests.
- Generates entity ids automatically on `Add` (supports both `Guid` ids and `GuidValueType`-based ids).
- Intended for testing only: data lives in process memory and is not persisted between instances, which is why providers are registered as singletons.

## Usage
### Registering the in-memory data provider
Use the `AddInMemoryDataProvider<TTvt>` extension to register the in-memory provider for a given data object type. It requires the `ITypeCache`, an `ExceptionBuilder`, and an `ILoggerFactory`.

```cs
using JLib.DataProvider.InMemory;
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var exceptions = new ExceptionBuilder("Setup");
var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

IServiceCollection services = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory, /* type packages */)
    .AddInMemoryDataProvider<EntityType>(typeCache, exceptions, loggerFactory);

exceptions.ThrowIfNotEmpty();
```

### Resolving and using a provider
Once registered, resolve `IDataProviderRw<TEntity>` (or the read-only `IDataProviderR<TEntity>`) and read/write entities. `Add` assigns the id automatically when it is not already set.

```cs
var provider = services.BuildServiceProvider();
var dataProvider = provider.GetRequiredService<IDataProviderRw<TestEntity>>();

var entity = new TestEntity();
dataProvider.Add(entity);

dataProvider.Get().Should().ContainSingle(t => t == entity);
```

### Registering a provider for a single entity type in tests
When you do not want to register providers for every discovered type, you can register the `InMemoryDataProvider<TEntity>` directly through `AddDataProvider`. This is the pattern used by the package's own tests.

```cs
IServiceCollection services = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory,
        new TypePackageBuilder()
            .AddNestedTypes<MyTestFixture>()
            .Build())
    .AddDataProvider<TestEntityType, InMemoryDataProvider<ITestEntity>, ITestEntity>(
        typeCache, null, null, null, exceptions, loggerFactory);
```

## Related Packages
- [JLib.DataProvider](../JLib.DataProvider/JLib.DataProvider%20Documentation.md) — defines the data provider abstractions (`IDataProviderR`/`IDataProviderRw`) and contains the `InMemoryDataProvider<TEntity>` implementation registered by this package.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — supplies the `ITypeCache` used to discover the data object types this provider is registered for.
