# JLib.HotChocolate Documentation

- DataProvider based Hot Chocolate Resolver implementations
    - 1:N: GetManyDataObjectsAsync
    - 1:1: GetOneDataObjectAsync
    - N:1: Not implemented yet
    - Implemented in `JLib.HotChocolate.Helper.ResolverContextHelper`
- TypeCache Support for GraphQl
    - TypeValueTypes
        - TypeExtension
            - Supports the Attributes `HotChocolate.Types.ExtendObjectTypeAttribute`, `HotChocolate.Types.ExtendObjectTypeAttribute<>` and the base class `HotChocolate.Types.ObjectTypeExtension`
            - Used by `JLib.HotChocolate.Helper.RequestExecutorBuilderHelper.AddTypeExtensions`