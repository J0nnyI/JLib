# JLib.AspNetCore

Helpers for building ASP.NET Core web applications, including `IServiceCollection` extension methods to register request scoped services that are instanced only once per HTTP request, even across manually created scopes.

## Installation
```sh
dotnet add package JLib.AspNetCore
```

## Features
- `AddRequestScoped` extension methods on `IServiceCollection` for registering services that are created **once per HTTP request** rather than once per DI scope.
- Unlike a normal scoped service, the instance is shared across any sub-scopes you create manually within the same request (e.g. one scope per worker thread).
- Overloads for open generic registration via `Type`, generic type parameters (`TService` / `TImplementation`), and a factory delegate (`Func<IServiceProvider, TService>`).
- Strongly typed, descriptive exceptions (deriving from `JLib.Exceptions.JLibException`) for misconfiguration and runtime failures.

## Usage

### Registering a request scoped service
Request scoped services require `IHttpContextAccessor` to be registered (the standard `AddHttpContextAccessor()` does this). The service is resolved through the HTTP request's `RequestServices`, so every manually created scope that shares the same `HttpContext` receives the same instance.

```cs
using JLib.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddHttpContextAccessor();

// once per request, even across nested scopes
services.AddRequestScoped<MyRequestState>();

// map a service to an implementation
services.AddRequestScoped<IRequestState, MyRequestState>();

// non-generic / runtime type registration
services.AddRequestScoped(typeof(MyRequestState));

// factory based registration
services.AddRequestScoped<IRequestState>(provider =>
    new MyRequestState(provider.GetRequiredService<ISomeDependency>()));
```

A typical use case is caching per-request information that is expensive to obtain, such as authentication details fetched from a database based on a bearer token. Resolving the service from the request scope or any sub-scope returns the exact same instance.

### Behaviour across nested scopes
The instance is anchored to the `HttpContext.RequestServices`. When you create additional scopes during request processing (for example to run work on multiple threads), they all resolve the same instance:

```cs
// within a request, the same Identifiyable instance is returned
// from the request scope and from any sub-scope sharing the HttpContext
var fromRequestScope = requestScope.ServiceProvider.GetRequiredService<Identifiyable>();
var fromSubScope     = subScope.ServiceProvider.GetRequiredService<Identifiyable>();
// fromRequestScope.ServiceId == fromSubScope.ServiceId

// a different request gets a different instance
var fromOtherRequest = otherRequestScope.ServiceProvider.GetRequiredService<Identifiyable>();
// fromOtherRequest.ServiceId != fromRequestScope.ServiceId
```

### Error handling
The extension methods throw specific exceptions, all derived from `AspNetCoreServiceCollectionExtensions.AddRequestScopedServiceException`:

- `UnsupportedGenericServiceException` (an `InitializationException`) — thrown at registration time when the implementation type is an open generic type definition, which is not supported.
- `MissingRequirementException` (a `RuntimeException`) — thrown when `IHttpContextAccessor` is not registered.
- `OutsideHttpContextScopeException` (a `RuntimeException`) — thrown when the service is resolved outside an HTTP request (i.e. when `HttpContext` is `null`, such as from the root/singleton scope).

```cs
try
{
    var state = provider.GetRequiredService<IRequestState>();
}
catch (AspNetCoreServiceCollectionExtensions.AddRequestScopedServiceException.OutsideHttpContextScopeException)
{
    // resolved outside of an active HTTP request
}
```

> Note: services registered via `AddRequestScoped` should be thread safe, since scopes are frequently created to spin up work on separate threads while sharing the same instance.

## Related Packages
- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) — base `JLibException` used by the request scoped service exceptions.
- [JLib.Helper](../JLib.Helper/JLib.Helper%20Documentation.md) — reflection and utility helpers used internally.
- [JLib.DependencyInjection](../JLib.DependencyInjection/JLib.DependencyInjection%20Documentation.md) — complementary DI helpers (e.g. `AddScopedAlias`) used alongside request scoped services.
