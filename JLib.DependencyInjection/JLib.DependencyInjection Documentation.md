# JLib.DependencyInjection

Dependency injection helpers for `Microsoft.Extensions.DependencyInjection`. Provides attribute-based service registration driven by the JLib reflection type cache (`ServiceAttribute` and `ServiceImplementationOverrideAttribute`), alias and generic service registration, and `IServiceProvider` extensions for lazy and multi-service resolution.

## Installation
```sh
dotnet add package JLib.DependencyInjection
```

## Features
- Attribute-based service registration via `AddServicesWithAttributes`, using the JLib `ITypeCache` to discover classes and interfaces decorated with `[Service]`.
- `ServiceAttribute` to declare a service lifetime on a class or interface, with automatic lifetime inference and validation (an implementation may not have a more restricted lifetime than its service interface).
- `ServiceImplementationOverrideAttribute` to pick a single implementation when multiple classes implement the same service interface(s).
- Alias registration (`AddAlias`, `AddTransientAlias`, `AddScopedAlias`, `AddSingletonAlias`) that provides one type under another service type while sharing the same instance.
- Generic registration helpers (`AddGenericServices`, `AddGenericAlias`) that register closed generic services for each matching type-value-type in the type cache.
- `AddScopeProvider` to inject an `IServiceScope` that returns the current provider (useful to detect or enforce a scoped provider).
- `IServiceProvider` extensions: `GetRequiredLazyService`/`GetLazyService`, `GetRequiredServices` (out-parameter and `params Type[]` overloads), and `GetServiceContainer` together with the `ServiceContainer<...>` records for bundling multiple services into a single parameter.

## Usage

### Registering services with attributes
Decorate classes and/or interfaces with `[Service(ServiceLifetime)]`, then register everything in the type cache with `AddServicesWithAttributes`. Validation errors are collected in an `ExceptionBuilder`, so call `ThrowIfNotEmpty` before building the provider.

```cs
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[Service(ServiceLifetime.Singleton)]
public class ShoppingService : IShoppingService { }

[Service(ServiceLifetime.Singleton)]
public interface IShoppingService { }

var exceptions = new ExceptionBuilder("startup");
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

var provider = new ServiceCollection()
    .AddTypeCache(out var typeCache, exceptions, loggerFactory)
    .AddServicesWithAttributes(typeCache, exceptions)
    .BuildServiceProvider();

exceptions.ThrowIfNotEmpty();

// IShoppingService is provided via an alias factory, so it resolves
// to the same singleton instance as ShoppingService.
var service = provider.GetRequiredService<IShoppingService>();
```

Registration rules enforced by `AddServicesWithAttributes`:
- `[Service]` on a class registers it as itself with the given lifetime.
- `[Service]` on an interface registers every implementing class under that interface; when the implementation has no `[Service]`, it inherits the interface's lifetime.
- When a class implements multiple service interfaces, the implementation and all interfaces resolve to the same instance.
- An implementation may not have a lower (more restricted) lifetime than a service interface it provides; doing so adds an exception to the builder.

### Choosing an implementation with an override
When several classes implement the same service interface, mark the preferred one with `[ServiceImplementationOverride]`. Passing a type (`[ServiceImplementationOverride(typeof(ShoppingService))]`) additionally validates that the override implements the same service interfaces, throwing if one is missing.

```cs
public class ShoppingService : IShoppingQueryService, IShoppingCommandService { }

[ServiceImplementationOverride]
public class MockShoppingService : IShoppingQueryService, IShoppingCommandService { }

[Service(ServiceLifetime.Singleton)]
public interface IShoppingQueryService { }

[Service(ServiceLifetime.Singleton)]
public interface IShoppingCommandService { }

// Both interfaces now resolve to MockShoppingService.
```

### Registering aliases manually
`AddAlias` registers one service type that resolves to an already-registered implementation via a factory, sharing the same instance.

```cs
services.AddSingleton<ShoppingService>();
services.AddSingletonAlias<IShoppingService, ShoppingService>();
// or with runtime types:
services.AddSingletonAlias(typeof(IShoppingService), typeof(ShoppingService));
```

### Resolving multiple services at once
`GetRequiredServices` provides `out`-parameter overloads (up to 20 services) and a `params Type[]` overload.

```cs
provider.GetRequiredServices(out IShoppingService shopping, out ILogger<Program> logger);

// or as a collection:
IReadOnlyCollection<object> services =
    provider.GetRequiredServices(typeof(IShoppingService), typeof(ILogger<Program>));
```

### Bundling services with a ServiceContainer
`ServiceContainer<...>` records bundle several services into a single resolvable type, reducing the number of constructor or method parameters. Resolve one with `GetServiceContainer` and access members via deconstruction.

```cs
public record ShoppingServices(/* marker */)
    : ServiceContainer<IShoppingQueryService, IShoppingCommandService>;

var (query, command) = provider.GetServiceContainer<ShoppingServices>();
```

### Lazy resolution
Defer resolving a service until it is first used.

```cs
Lazy<IShoppingService> lazy = provider.GetRequiredLazyService<IShoppingService>();
Lazy<IShoppingService?> optional = provider.GetLazyService<IShoppingService>();
```

### Generic service registration
`AddGenericServices` and `AddGenericAlias` register closed generic services/aliases for each type-value-type in the type cache that matches an optional filter, resolving the generic type arguments from the value type.

```cs
services.AddGenericServices<MyTypeValueType, IRepository<object>, Repository<object>>(
    typeCache,
    ServiceLifetime.Scoped,
    exceptions,
    loggerFactory);
```

## Related Packages
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) - provides the `ITypeCache` and `AddTypeCache` that drive attribute-based registration.
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) - provides the `ExceptionBuilder` used to collect setup errors.
- [JLib.Configuration](../JLib.Configuration/JLib.Configuration%20Documentation.md) - options/configuration helpers used alongside DI registration.
- [JLib.AutoMapper](../JLib.AutoMapper/JLib.Automapper%20Documentation.md) - AutoMapper integration that pairs with this package's registration.
