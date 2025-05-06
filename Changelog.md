# Current
## Relevant Changes
### Jlib.Reflection
#### Type Package Improvements
- TypePackageBuilder added
    - Significantly improved performance over the previous implementation
- TypePackage Deprectaed
    - The Builder should be used instead
### JLib.Configuration
#### Environemnt
- The Environment key can now be changed, to be able to not use it. The default value is set by vs per default.


## Bug Fixes
### JLib.DataProvider
- Fixed missing ScopeProvider in AuthorizationExtensions.AddDataAuthorization
### JLib.DependencyInjection
- AddScopeProvider now handles multiplle calls gracefully


## Documentation
### Changelog.md
- restructured format to allow for change prioritization