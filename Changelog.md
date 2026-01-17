# 0.13.0
## Breaking Changes
### JLib.Reflection
- removed all typePackage usages and implementations. Use the TypePackageBuilder instead.
  - calls will be cached by default. 

### JLib.Helper
- changed ReflectionHelper attribute method declarations to cache the result.

### JLib.Helper
- changed ReflectionHelper attribute method declarations to cache the result.

### JLib.ValueTypes.*
- renamed JLib.ValueTypes.Mapping -> JLib.ValueTypes.AutoMapper
- moved valueType json converters to JLib.ValueTypes package

## Features
### JLib.Helper
- AttributeCache added

# 0.12.0
## Breaking Changes
### Package References
- Updated all references, including automapper, which causes an indirect breaking change.
## Features
### DotNet
- Added support for .NET 10
### JLib.Reflection
#### Type Package
- TypePackageBuilder added
    - Significantly improved performance over the previous implementation
- TypePackage Deprectaed
    - The Builder should be used instead
### JLib.Configuration
#### Environment
- The Environment key can now be changed, to be able to not use it. The default value is set by vs per default.

## Improvements
### Jlib.Reflection
#### Typecache
- added details to thrown exceptions
### JLib.DataGeneration
- AutoMapper References removed


