# JLib.Exceptions

Provides a standardized way to create, collect, and aggregate exceptions. The `ExceptionBuilder` and `IExceptionProvider` abstractions let you accumulate multiple errors across nested scopes and throw them as a single `JLibAggregateException` with a readable, hierarchical message.

## Installation
```sh
dotnet add package JLib.Exceptions
```

## Features
- `ExceptionBuilder` for collecting multiple exceptions (and nested child builders) and throwing them as one aggregate only when at least one error exists.
- `JLibAggregateException`, an `AggregateException` whose `Message` renders a human-readable, hierarchical JSON tree of all contained exceptions while preserving the original `UserMessage`.
- `IExceptionProvider` abstraction so validators and other components can be attached to a builder and evaluated lazily, plus ready-made implementations `ConstantExceptionProvider` and `EmptyExceptionProvider`.
- `IDisposable` support on `ExceptionBuilder`: a root builder throws on dispose if it collected errors; an empty child builder removes itself from its parent.
- Extension methods (`ToProvider`, `ThrowExceptionIfNotEmpty`, `GetExceptionIfNotEmpty`, `AddIfNotEmpty`, `GetHierarchyInfo`, `GetHierarchyInfoJson`, `ToHumanOptimizedJsonObject`) for working with exception collections.
- `JLibException` base class for library exceptions, including `InvalidSetupException` / `IInvalidSetupException` and `MaxIterationDepthReachedException`.

## Usage

### Collecting and throwing multiple errors
Create an `ExceptionBuilder`, add as many exceptions as you like, then call `ThrowIfNotEmpty`. If nothing was added, nothing is thrown.

```cs
using JLib.Exceptions;

var exceptions = new ExceptionBuilder("Example");

// does nothing since no exception was added
exceptions.ThrowIfNotEmpty();

exceptions.Add(new Exception("Example Exception"));
exceptions.Add("a message is wrapped in a plain Exception");

// throws a JLibAggregateException containing both errors
exceptions.ThrowIfNotEmpty();
```

### Nested exceptions
Use `CreateChild` to group related errors under a sub-message. Children are evaluated when the parent builds its exception.

```cs
var exceptionBuilder = new ExceptionBuilder("Example");
exceptionBuilder.Add(new Exception("Example Exception"));

var child = exceptionBuilder.CreateChild("Children");
child.Add(new Exception("Exceptions of the child"));

exceptionBuilder.ThrowIfNotEmpty(); // throws JLibAggregateException
```

### Disposable builders
A root builder used with `using` calls `ThrowIfNotEmpty` on dispose, so errors are thrown automatically when the scope ends. An empty child builder simply removes itself from its parent on dispose.

```cs
using var exceptionBuilder = new ExceptionBuilder("Example");
exceptionBuilder.Add(new Exception("ExampleException"));
// when the using scope ends, a JLibAggregateException is thrown
```

### Building an exception without throwing
`GetException` returns the aggregate (or `null` when there are no errors) so you can inspect or store it.

```cs
var exceptionBuilder = new ExceptionBuilder("Example");
exceptionBuilder.Add(new Exception("Example Exception"));

Exception? exception = exceptionBuilder.GetException(); // JLibAggregateException
bool hasErrors = exceptionBuilder.HasErrors();          // true
```

### Lazy validation with IExceptionProvider
Implement `IExceptionProvider` so a validator can be attached to a builder before its work is finished; `GetException` is invoked lazily when the parent builds its result. Attach it with `AddChild` / `AddChildren`.

```cs
public class ExampleExceptionProvider : IExceptionProvider
{
    private readonly bool _isValid;
    public ExampleExceptionProvider(bool isValid) => _isValid = isValid;

    public Exception? GetException()
        => _isValid ? null : new Exception("Data is Invalid");

    public bool HasErrors() => !_isValid;
}

var exceptionBuilder = new ExceptionBuilder("Example");
exceptionBuilder.AddChild(new ExampleExceptionProvider(isValid: false));

var exception = exceptionBuilder.GetException(); // JLibAggregateException
```

Use `EmptyExceptionProvider.Instance` when a provider is required but never has errors, `new ConstantExceptionProvider(exception)` to wrap a fixed exception, or `exception.ToProvider()` to adapt an existing `Exception`.

### Throwing directly from an exception collection
Extension methods let you aggregate and throw straight from an `IEnumerable<Exception>`.

```cs
using JLib.Exceptions;

IEnumerable<Exception> errors = Validate(input);

// throws JLibAggregateException only when errors is not empty
errors.ThrowExceptionIfNotEmpty("validation failed");

// or build it without throwing
Exception? aggregate = errors.GetExceptionIfNotEmpty("validation failed");

// or collect it into a master list
var masterList = new List<Exception>();
errors.AddIfNotEmpty("validation failed", masterList);
```

`JLibAggregateException` also exposes static `ThrowIfNotEmpty` / `ReturnIfNotEmpty` helpers for the same purpose.

### Readable exception output
`JLibAggregateException.Message` is rendered as a human-optimized JSON tree. You can produce the same output for any exception via the extension methods.

```cs
string json = aggregate.GetHierarchyInfoJson();   // grouped, indented JSON
string tree = aggregate.GetHierarchyInfo();        // text tree (AggregateException)
```

## Related Packages
- [JLib.Helper](../JLib.Helper/JLib.Helper%20Documentation.md) — extension and utility methods this package builds on.
