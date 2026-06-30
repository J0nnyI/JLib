# JLib.Helper

Foundational helper and extension methods for .NET, covering reflection and `Type` inspection, enumerables, dictionaries, hash sets, strings, chars, expressions, nullable handling, object casting, exceptions, and disposables. It serves as the shared utility base across the JLib libraries.

## Installation
```sh
dotnet add package JLib.Helper
```

## Features
- **Type inspection** (`TypeHelper`): primitive/numeric/nullable checks, inheritance and interface inspection ignoring generic type parameters, instantiability checks, generic-definition helpers, and cached human-readable type and method names.
- **Reflection** (`ReflectionHelper`): access-modifier detection, init-only property detection, nullable-reference-type detection, attribute lookups backed by a cache, and code-like debug representations of members.
- **Attribute caching** (`ICustomTypeAttributeCache` / `CustomTypeAttributeCache`): a thread-safe cache that avoids re-evaluating custom attributes on each reflection call.
- **Enumerable extensions** (`EnumerableExtensions`): `Multiple`, `WhereNotNull`, `None`, `AddIndex`, in-place `RemoveWhere`/`Remove`, and conversions to read-only and concurrent dictionaries.
- **Dictionary extensions** (`DictionaryHelper`): `GetValueOrAdd`, `AddOrReplace`, `AddRange`/`RemoveRange`, and a null-returning `TryGetValue`.
- **HashSet extensions** (`HashSetExtensions`): `AddRange` and `RemoveRange`.
- **String/char/StringBuilder helpers** (`StringHelper`, `CharHelper`): `IsNullOrWhitespace`, `IsNullOrEmpty`, `Repeat`, `SubStringUntil`, `RemoveSubstringsWhere`, `AppendMultiple`, and `char` predicates.
- **Expression helpers** (`ExpressionHelper`): convert expressions to nullable, extract `PropertyInfo` from a property lambda, and replace method calls inside an expression tree.
- **Object casting** (`ObjectCastExtensions`): fluent `CastTo<T>` and `As<T>`.
- **Exception helpers** (`ExceptionHelper`): conditional `Throw` and recursive `FlattenAll`.
- **Disposable helpers** (`DisposableHelper`): `DisposeWith` and `DisposeAll`.

## Usage

### Inspecting types and generics
`TypeHelper` adds checks that treat open generic definitions consistently, so you can ask whether a type implements or derives from a generic interface or base type without specifying its type arguments.

```cs
using JLib.Helper;

typeof(int).IsNumber();              // true
typeof(Guid?).IsNullableGuid();      // true
typeof(List<string>).ImplementsAny<IEnumerable<int>>(); // true (type params ignored)
typeof(MyService).IsInstantiable();  // false for static/abstract/interface types

// Filter a set of types
IEnumerable<Type> handlers = allTypes
    .WhichImplementAny<IHandler<object>>()
    .WhichAreInstantiable();

// Human-readable, cached names
string name = typeof(Dictionary<string, List<int>>).FullName();
// "Dictionary<string, List<int>>"
```

### Reflecting over members and attributes
`ReflectionHelper` exposes attribute lookups that are backed by a shared `CustomTypeAttributeCache`, along with access-modifier and nullability detection.

```cs
using JLib.Helper;

bool hasAttr = typeof(MyType).HasCustomAttribute<ObsoleteAttribute>();
IReadOnlyCollection<ObsoleteAttribute> attrs =
    typeof(MyType).GetCustomAttributes<ObsoleteAttribute>();

PropertyInfo prop = typeof(MyType).GetProperty(nameof(MyType.Value))!;
bool isInit = prop.IsInit();       // true for { get; init; }
bool isNullable = prop.IsNullable();

AccessModifier modifier = methodInfo.GetAccessModifier();
```

You can also use a dedicated cache instance directly when you want to manage its lifetime:

```cs
ICustomTypeAttributeCache cache = new CustomTypeAttributeCache();
bool defined = cache.IsDefined<ObsoleteAttribute>(typeof(MyType));
cache.Clear(typeof(ObsoleteAttribute)); // invalidate a single attribute type
```

### Working with enumerables and dictionaries
```cs
using JLib.Helper;

bool more = items.Multiple();                 // true if at least 2 elements
var nonNull = source.WhereNotNull();          // drops null entries
bool empty = items.None();

// In-place removal (also available for IList<T> and ConcurrentDictionary<,>)
list.RemoveWhere(x => x.IsExpired);

var concurrent = items.ToConcurrentDictionary(x => x.Id);
var value = dict.GetValueOrAdd(key, () => Compute(key));
```

### Manipulating strings
`RemoveSubstringsWhere` splits a string by a separator and drops the substrings that match a predicate. A three-argument overload exposes the previous, current and next substring.

```cs
using JLib.Helper;

"1,2,3".RemoveSubstringsWhere(x => x == "2", ",");          // "1,3"
"1,2,3".RemoveSubstringsWhere((prev, cur, next) => cur == "1", ","); // "2,3"

"ab".Repeat(3);                  // "ababab"
"file.txt".SubStringUntil('.');  // "file"
```

### Replacing method calls in expression trees
`ReplaceMethod` swaps every call to a given method inside an expression with a replacement lambda, validating parameter and return types.

```cs
using JLib.Helper;

Expression<Func<Order, decimal>> source = o => Discount(o.Total);
Expression<Func<decimal, decimal>> replacement = total => total * 0.9m;

var rewritten = source.ReplaceMethod(
    typeof(MyClass).GetMethod(nameof(Discount))!,
    replacement);

// Extract a PropertyInfo from a property lambda
PropertyInfo info = ((Expression<Func<Order, decimal>>)(o => o.Total)).GetPropertyInfo();
```

### Exceptions and disposables
```cs
using JLib.Helper;

maybeException.Throw();                    // throws only if not null
var all = aggregate.FlattenAll().ToList(); // flattens nested/inner exceptions

var stream = new MemoryStream().DisposeWith(disposables);
disposables.DisposeAll();
```

## Related Packages
JLib.Helper has no JLib dependencies and forms the shared utility base for the rest of the JLib libraries, including:
- [JLib.Reflection](../JLib.Reflection/JLib.Reflection%20Documentation.md)
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md)
- [JLib.Configuration](../JLib.Configuration/JLib.Configuration%20Documentation.md)
