# JLib.Testing

Testing utilities for the JLib ecosystem that reduce boilerplate in snapshot tests. Provides [Snapshooter](https://github.com/SwissLife-OSS/snapshooter)-based extensions to prepare JLib type value types, service collections, and exceptions for stable snapshot matching with xUnit.

## Installation
```sh
dotnet add package JLib.Testing
```

## Features
- `PrepareSnapshot` for `IEnumerable<ITypeValueType>`: groups type value types by namespace and then by `TypeValueType` kind, producing a deterministic, JSON-friendly object ready for `MatchSnapshot` (no `SnapshotNameExtension` required).
- `PrepareSnapshot` for `IServiceCollection`: transforms a service collection into a stable object describing each service's lifetime, service type, implementation type, generic arguments, and implementation source (`Type`, `Instance`, or `Factory`) while excluding volatile implementation details.
- `PrepareSnapshot` for `Exception`: converts an exception (including `AggregateException` / `JLibAggregateException`) into an `ExceptionSnapshotInfo` record with type, message lines, and recursively ordered inner exceptions.
- `ExceptionSnapshotInfo`: an immutable `record struct` capturing exception type, split message lines, and inner exceptions in a deterministic order.

## Usage

### Snapshotting a service collection
`IServiceCollection.PrepareSnapshot()` builds a stable representation of all registered services so dependency-injection setups can be locked down with a snapshot. Note that it builds a provider and creates a scope to resolve factory-based registrations.

```cs
using JLib.Testing;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit;

public class ServiceRegistrationTests
{
    [Fact]
    public void ServicesMatchSnapshot()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMyService, MyService>();

        services.PrepareSnapshot().MatchSnapshot();
    }
}
```

### Snapshotting cached type value types
`IEnumerable<ITypeValueType>.PrepareSnapshot()` groups the reflection cache contents by namespace and `TypeValueType`, producing deterministic output suitable for verifying which types were discovered.

```cs
using JLib.Reflection;
using JLib.Testing;
using Snapshooter.Xunit;

cache.All<ITypeValueType>()
    .Where(tvt => tvt.Value.Assembly != typeof(ITypeCache).Assembly)
    .PrepareSnapshot()
    .MatchSnapshot();
```

### Snapshotting exceptions
`Exception.PrepareSnapshot()` returns an `ExceptionSnapshotInfo?` (null for a null exception), unwrapping `JLibAggregateException.UserMessage` and ordering nested `AggregateException` inner exceptions for stable comparison.

```cs
using JLib.Testing;

object validator;
try
{
    validator = services.PrepareSnapshot();
}
catch (Exception e)
{
    validator = e.PrepareSnapshot() as object ?? "evaluation failed";
}
```

### Combining preparations in a single snapshot
The prepared objects can be combined into a dictionary and matched in one call, e.g. to validate a reflection cache, its services, and any captured exceptions together.

```cs
using JLib.Testing;
using Snapshooter;
using Snapshooter.Xunit;

new Dictionary<string, object?>
{
    { "Exceptions", exceptions?.GetException()?.PrepareSnapshot() },
    { "CachedTypes", cache.All<ITypeValueType>().PrepareSnapshot() },
}.MatchSnapshot(new SnapshotNameExtension(testName));
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — provides `ITypeValueType`, `ITypeCache`, and the type-value-type model these snapshot helpers operate on.
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) — provides `JLibAggregateException`, used by `ExceptionSnapshotInfo` when extracting user messages.
- [JLib](../JLib/JLib%20Documentation.md) — provides the `JLib.Helper` extensions (e.g. `FullName`, `As`) used throughout the snapshot preparation.
