# JLib.DataGeneration

Manage test data using composable **data packages** with deterministic, persistent IDs. The package provides the `DataPackage` base type, an ID registry that stores generated `int` and `Guid` values in a file next to the project, and a stacktrace-based ID generator for IDs created at runtime. It integrates with the JLib TypeCache and AutoMapper for strongly typed value-type IDs.

## Installation
```sh
dotnet add package JLib.DataGeneration
```

## Features
- **Data packages** - derive from `DataPackage` to declare reusable, composable sets of test data. Packages are resolved through Dependency Injection and can include each other via `IncludeDataPackages`.
- **Persistent property IDs** - public ID properties on a package (`Guid`, `int`, or a `GuidValueType` / `IntValueType` / `StringValueType` derivative) are assigned the same value on every run. The values are persisted to a file stored next to the project.
- **Runtime IDs** - `TestingIdGenerator` derives stable IDs from the caller's stack trace (declaring type, method, generic argument count and parameters), so IDs created at runtime (e.g. when inserting new entities) stay deterministic across runs.
- **ID scopes** - `SetIdScope` partitions the runtime call counter so unrelated test sections do not influence each other's IDs.
- **Strongly typed IDs** - via AutoMapper, generated `int`/`Guid`/`string` values are mapped to your own `IntValueType`, `GuidValueType`, and `StringValueType` records.
- **Reverse lookup / debugging** - `IdInfo`, `IdInfoObj` and `IdSnapshot` extension methods resolve a raw id value back to the package property or call site that produced it.
- **Readable identifiers** - `IdRegistryConfiguration.NamespaceAliases` shorten long namespaces in the persisted ID names.
- **Production abstraction** - the `JLib.DataGeneration.Abstractions` package exposes `IIdGenerator` so production code can request IDs without depending on the test infrastructure.

## Concepts
### Property IDs
A property ID is declared by adding a public property with a public getter and a public `init` setter to a `DataPackage`. The type must be `Guid`, `int`, or a derivative of `GuidValueType` / `IntValueType` / `StringValueType`. On initialization, each ID property is populated from the ID registry; if no value exists yet, a new one is created and persisted. To reference an entity from another package, inject that package and read its property.

Properties that should *not* receive a generated ID can be marked with `[SkipIdAssignment]`.

A `DataPackage` must be `sealed` or `abstract`.

### Runtime IDs
When IDs are created at runtime (for example, when production code creates a new entity), inject `TestingIdGenerator` and call `CreateGuid`, `CreateIntId`, or `CreateStringId`. The generator uses the caller's stack frame as the identity, so the same call site yields the same ID across runs.

### Namespace aliases
`IdRegistryConfiguration.NamespaceAliases` replace long namespaces with a short alias (`~Alias~`) in the persisted identifiers, keeping the registry file readable.

## Usage
### 1. Register data packages
Add the DataGeneration assembly to the `ITypePackage`, then register AutoMapper and the data packages. `AddDataPackages` adds the ID registry and the `TestingIdGenerator` for you.

```cs
using JLib.AutoMapper;
using JLib.DataGeneration;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var exceptions = new ExceptionBuilder("test setup");

var typePackage = new TypePackageBuilder()
    .AddAssemblyOf<DataPackage>()
    .Build();

var provider = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, typePackage)
    .AddAutoMapper(c => c.AddProfiles(typeCache))
    .AddDataPackages(typeCache, new IdRegistryConfiguration
    {
        NamespaceAliases = new[] { new NamespaceAlias("MyCompany.MyApp") }
    })
    .BuildServiceProvider();

exceptions.ThrowIfNotEmpty();
```

### 2. Declare a data package with deterministic IDs
Add a public ID property and use `GetInfoText` to produce a human-readable label for the generated entity.

```cs
using System.ComponentModel.DataAnnotations;
using JLib.DataGeneration;
using JLib.ValueTypes;

// strongly typed id
public record ArticleId(Guid Value) : GuidValueType(Value);

public sealed class ArticleDataPackage : DataPackage
{
    public ArticleId ArticleId { get; init; } = null!;

    public ArticleDataPackage(IServiceProvider serviceProvider, ShopDbContext dbContext)
        : base(serviceProvider)
    {
        dbContext.Articles.Add(new Article
        {
            Id = ArticleId.Value,
            Name = this.GetInfoText(x => x.ArticleId)
        });
    }
}
```

### 3. Load packages and read their IDs
Load packages with `IncludeDataPackages` (this is what runs their constructors), then resolve them to read the deterministic IDs.

```cs
provider.IncludeDataPackages<ArticleDataPackage>();

var package = provider.GetRequiredService<ArticleDataPackage>();
package.ArticleId.Should().NotBeNull();
```

A package can also pull in its dependencies from within its own constructor:

```cs
public sealed class OrderDataPackage : DataPackage
{
    public OrderDataPackage(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // ensures ArticleDataPackage is initialized and its ids are available
        IncludeDataPackages<ArticleDataPackage>();
    }
}
```

### 4. Generate IDs at runtime
Inject `TestingIdGenerator` to create IDs whose values are stable per call site. Use `SetIdScope` to isolate counters between sections of a test.

```cs
var idGenerator = provider.GetRequiredService<TestingIdGenerator>();

Guid raw = idGenerator.CreateGuid();
ArticleId typed = idGenerator.CreateGuid<ArticleId>();
int intId = idGenerator.CreateIntId();

idGenerator.SetIdScope(new DataPackageValues.IdScopeName("import"));
// ids created here are counted separately from the default scope
```

### 5. Reverse-lookup an id while debugging
Resolve a raw id value back to the package property or call site that produced it.

```cs
using JLib.DataGeneration;

string info = someGuid.IdInfo(idRegistry);
// e.g. "Guid [MyApp.ArticleDataPackage].[ArticleId] = 8ca6e4e4-..."

IdInformation structured = someGuid.IdInfoObj(idRegistry);
IdSnapshotInformation snapshot = someGuid.IdSnapshot(idRegistry);
```

### Production code (Abstractions)
In production code, depend only on `IIdGenerator` from `JLib.DataGeneration.Abstractions` and register the runtime implementation with `AddIdGenerator`. While testing, `AddDataPackages` (or `AddTestingIdGenerator`) registers the deterministic `TestingIdGenerator` as the `IIdGenerator` instead.

```cs
using JLib.DataGeneration.Abstractions;

services.AddIdGenerator(); // registers the production IdGenerator
```

## Related Packages
- [JLib.DataGeneration.Abstractions](../JLib.DataGeneration.Abstractions/JLib.DataGeneration.Abstractions%20Documentation.md) - the `IIdGenerator` abstraction used by production code.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) - provides the `ITypeCache` that discovers `DataPackage` types.
- [JLib.DependencyInjection](../JLib.DependencyInjection/JLib.DependencyInjection%20Documentation.md) - DI helpers used to register and resolve packages.
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) - base types (`GuidValueType`, `IntValueType`, `StringValueType`) for strongly typed IDs.
- [JLib.AutoMapper](../JLib.AutoMapper/JLib.Automapper%20Documentation.md) - maps generated primitive ids to your typed id value types.
