# JLib.ValueTypes

Core building blocks for strongly-typed value types in .NET. JLib.ValueTypes provides base classes (such as `StringValueType` and the numeric value types) together with an attribute-based validation framework that wraps and validates primitive values. Other JLib packages build on this foundation; JSON converters are provided separately (System.Text.Json support lives in `JLib.SystemTextJson`).

## Installation

```sh
dotnet add package JLib.ValueTypes
```

## Features

- Strongly-typed wrappers around primitives via the abstract `record ValueType<T>` base class.
- Ready-to-use base classes for common native types: `StringValueType`, `GuidValueType`, `CharValueType`, and the numeric types (`IntValueType`, `LongValueType`, `DecimalValueType`, `DoubleValueType`, `FloatValueType`, `ByteValueType`, `SByteValueType`, `ShortValueType`, `UShortValueType`, `UIntValueType`, `ULongValueType` — all derived from `NumericValueType<T>`).
- Attribute-based validation: mark a `static` method with `[Validation]` and it is invoked whenever the value type (or any derivation of it) is constructed.
- A fluent `IValidationContext<T>` / `ValidationContext<T>` API with built-in validation extension methods for strings (`Contain`, `EndWith`, `NotBeNull`, `NotBeNullOrEmpty`, `BeOneOf`, `BeAlphanumeric`, `SatisfyCondition`, ...) and integers (`BeGreaterThan`, `BeLessThan`, `BeInBounds`, `BePositive`, `NotBeNegative`, ...).
- Validation inheritance: a derived value type runs both its own and all base-type validations.
- Static helpers on the `ValueType` class for validating and constructing instances reflectively: `Validate`, `GetErrors`, `Create`, `CreateNullable`, `TryCreate`, and `ValidateValueTypeSetup`.
- Validation failures throw a `JLibAggregateException` (from `JLib.Exceptions`) collecting all errors at once.

## Usage

### Defining a value type with validation

Derive from one of the base records and register a validation method with `[Validation]`. The method is `static`, takes a `ValidationContext<T>` (or `IValidationContext<T>`), and is called automatically on construction.

```cs
using JLib.ValueTypes;

public record EmailAddress(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(IValidationContext<string?> must)
        => must.Contain("@").Contain(".");
}

// valid
var email = new EmailAddress("my@example.com");

// invalid -> throws JLibAggregateException
var invalid = new EmailAddress("not an email address");
```

### Inheriting validation from another value type

When one value type derives from another, the validations of both the derived and the base type are applied.

```cs
public record EmailAddress(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string?> must)
        => must.Contain("@").Contain(".");
}

public record GermanEmailAddress(string Value) : EmailAddress(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string?> must)
        => must.EndWith(".de");
}

new GermanEmailAddress("my@example.de"); // valid
new GermanEmailAddress("my@example.com"); // fails GermanEmailAddress validation
new GermanEmailAddress("example.de");     // fails EmailAddress (base) validation
```

### Validating without constructing (static helpers)

Use the static methods on `ValueType` to check a value or build an instance safely.

```cs
using ValueType = JLib.ValueTypes.ValueType;

// collect validation errors without throwing
IExceptionProvider errors = ValueType.GetErrors<EmailAddress, string>("not a mail");
bool isInvalid = errors.HasErrors();

// quick boolean check
bool ok = ValueType.Validate<EmailAddress, string>("my@example.de");

// try to create: returns null and exposes the errors on failure
EmailAddress? value = ValueType.TryCreate<EmailAddress, string>(
    "my@example.com", out var validationErrors);
```

### Reusing validation logic with extension methods

Add reusable rules as extension methods on `ValidationContext<T>` / `IValidationContext<T>` and chain them fluently.

```cs
public static class EmailValidation
{
    public static IValidationContext<string?> NotEndWithDot(this IValidationContext<string?> ctx)
    {
        if (ctx.Value?.EndsWith(".") != false)
            ctx.AddError($"'{ctx.Value}' must not end with '.'");
        return ctx;
    }
}

public record EmailAddress(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string?> must)
        => must.Contain("@").Contain(".").NotEndWithDot();
}
```

## Related Packages

- [JLib.Exceptions](../JLib.Exceptions/JLib.Exceptions%20Documentation.md) — provides `IExceptionProvider`, `ExceptionBuilder`, and `JLibAggregateException` used by the validation pipeline (dependency).
- [JLib.Helper](../JLib.Helper/JLib.Helper%20Documentation.md) — reflection and general helper extensions used internally (dependency).
- [JLib.SystemTextJson](../JLib.SystemTextJson/JLib.SystemTextJson%20Documentation.md) — System.Text.Json converters for value types.
- [JLib.ValueTypes.AutoMapper](../JLib.ValueTypes.AutoMapper/JLib.ValueTypes.AutoMapper%20Documentation.md) — AutoMapper integration for value types.
- [JLib.ValueTypes.Implementations.FileSystem](../JLib.ValueTypes.Implementations.FileSystem/JLib.ValueTypes.Implementations.FileSystem%20Documentation.md) — ready-made file-system path value types built on this package.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — reflection/type-package model that discovers and registers value types.
