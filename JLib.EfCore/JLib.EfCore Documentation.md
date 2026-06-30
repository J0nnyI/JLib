# JLib.EfCore

Provides Entity Framework Core `ValueConverter`s for JLib ValueTypes, allowing strongly typed ValueTypes (backed by class or struct values) to be used directly as properties on EF Core entities.

## Installation
```sh
dotnet add package JLib.EfCore
```

## Features
- `ClassValueTypeValueConverter<TValueType, TValue>` — converts a nullable ValueType backed by a reference type (e.g. `string`) to and from its underlying value.
- `StructValueTypeValueConverter<TValueType, TValue>` — converts a nullable ValueType backed by a value type (e.g. `int`, `Guid`) to and from its underlying nullable value.
- `StructNonNullableValueTypeValueConverter<TValueType, TValue>` — converts a non-nullable ValueType backed by a value type to and from its underlying value.
- Constructs each `TValueType` via its single-argument constructor (`TValueType(TValue)`), so ValueType validation runs when EF Core materializes entities.
- Null-safe: the class and nullable-struct converters map `null` underlying values to `null` ValueTypes and vice versa.

## Usage
### Defining ValueTypes
The converters target types deriving from `JLib.ValueTypes.ValueType<T>`, which must expose a constructor accepting a single `T` value.

```cs
using JLib.ValueTypes;

public record UserId(Guid Value) : ValueType<Guid>(Value);
public record UserName(string Value) : ValueType<string>(Value);
```

### Configuring converters in your DbContext
Apply the matching converter to each property based on whether the underlying value is a reference type, a value type, or a non-nullable value type.

```cs
using JLib.EfCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();

        // non-nullable struct-backed ValueType (Guid)
        user.Property(u => u.Id)
            .HasConversion(new StructNonNullableValueTypeValueConverter<UserId, Guid>());

        // class-backed ValueType (string)
        user.Property(u => u.Name)
            .HasConversion(new ClassValueTypeValueConverter<UserName, string>());

        // nullable struct-backed ValueType (int?)
        user.Property(u => u.Age)
            .HasConversion(new StructValueTypeValueConverter<Age, int>());
    }
}

public record Age(int Value) : ValueType<int>(Value);

public class User
{
    public UserId Id { get; set; } = null!;
    public UserName Name { get; set; } = null!;
    public Age? Age { get; set; }
}
```

### Choosing the right converter
- Underlying value is a reference type (`class`, e.g. `string`): use `ClassValueTypeValueConverter<TValueType, TValue>`.
- Underlying value is a value type (`struct`, e.g. `int`, `Guid`) and the property is nullable: use `StructValueTypeValueConverter<TValueType, TValue>`.
- Underlying value is a value type and the property is required/non-nullable: use `StructNonNullableValueTypeValueConverter<TValueType, TValue>`.

## Related Packages
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) — defines the `ValueType<T>` base type these converters operate on.
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md) — referenced for type discovery infrastructure.
