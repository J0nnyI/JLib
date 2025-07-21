using JLib.Reflection;
using JLib.ValueTypes;

namespace JLib.DataProvider;

/// <summary>
/// a class which directly accesses data using EfCore, a web api or other methods
/// </summary>
/// <param name="Value"></param>
[TvtFactoryAttribute.Implements(typeof(IEntity)), TvtFactoryAttribute.IsClass, TvtFactoryAttribute.NotAbstract]
public record EntityType(Type Value) : DataObjectType(Value), IValidatedType
{
    public new const int NextPriority = DataObjectType.NextPriority - 1_000;

    public virtual void Validate(ITypeCache cache, IValidationContext<Type> value) { }
}