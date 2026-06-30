# JLib.DataGeneration.Abstractions

Provides the `IIdGenerator` abstraction for generating `Guid`s and `GuidValueType` identifiers at runtime, registrable via `AddIdGenerator` on the dependency injection container. The default implementation can be replaced by the `JLib.DataGeneration` package during testing to produce trackable, deterministic IDs.

## Installation
```sh
dotnet add package JLib.DataGeneration.Abstractions
```

## Features
- `IIdGenerator` abstraction for creating new identifiers without depending on a concrete implementation.
- Generates plain `Guid`s as well as strongly typed `GuidValueType` identifiers (from `JLib.ValueTypes`).
- Default runtime `IdGenerator` implementation backed by `Guid.NewGuid()`.
- One-line DI registration via `AddIdGenerator`, registering the generator as a singleton.
- Designed to be swapped out at test time (e.g. by `JLib.DataGeneration`) to emit trackable, deterministic IDs.

## Usage

### Registering the runtime ID generator
Register the default implementation on the DI container during application startup.

```cs
using JLib.DataGeneration.Abstractions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddIdGenerator();
```

### Generating identifiers
Inject `IIdGenerator` and use it to create plain or strongly typed identifiers.

```cs
using JLib.DataGeneration.Abstractions;
using JLib.ValueTypes;

// a strongly typed identifier defined in your domain
public record OrderId(Guid Value) : GuidValueType(Value);

public class OrderFactory
{
    private readonly IIdGenerator _idGenerator;

    public OrderFactory(IIdGenerator idGenerator)
        => _idGenerator = idGenerator;

    public OrderId NewOrderId()
    {
        // plain Guid
        Guid raw = _idGenerator.CreateGuid();

        // strongly typed GuidValueType
        return _idGenerator.CreateGuid<OrderId>();
    }
}
```

### Swapping the implementation while testing
During tests, the `JLib.DataGeneration` package replaces the runtime generator with one that produces trackable, deterministic IDs. Use `AddTestingIdGenerator` (from `JLib.DataGeneration`) instead of `AddIdGenerator` in the test setup; consumers still depend only on `IIdGenerator`.

```cs
using JLib.DataGeneration; // provides AddTestingIdGenerator
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTestingIdGenerator();
```

## Related Packages
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) — provides `GuidValueType`, the base record for strongly typed GUID identifiers.
- [JLib.DataGeneration](../JLib.DataGeneration/JLib.DataGeneration%20Documentation.md) — replaces the default `IIdGenerator` with a testing implementation that emits trackable, deterministic IDs.
