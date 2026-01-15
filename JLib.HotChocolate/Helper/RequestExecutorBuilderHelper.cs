using System.Reflection;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using JLib.DataProvider;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Helper;

/// <summary>
/// a type which extends a given graphql object.
/// </summary>
/// <seealso cref="AttributeTypeExtensionType"/>
/// <seealso cref="ClassTypeExtensionType"/>
public abstract record TypeExtensionType : TypeValueType
{
    /// <summary>
    /// <inheritdoc cref="TypeExtensionType"/>
    /// </summary>
    /// <param name="Value"></param>
    internal TypeExtensionType(Type Value) : base(Value)
    {
    }
}
/// <summary>
/// a type which extends a given graphql object.
/// </summary>
[TvtFactoryAttribute.IsClass, TvtFactoryAttribute.HasAttribute(typeof(ExtendObjectTypeAttribute))]
public record AttributeTypeExtensionType(Type Value) : TypeExtensionType(Value);
/// <summary>
/// a type which extends a given graphql object.
/// </summary>
[TvtFactoryAttribute.IsClass, TvtFactoryAttribute.HasAttribute(typeof(ExtendObjectTypeAttribute<>))]
public record GenericAttributeTypeExtensionType(Type Value) : TypeExtensionType(Value);


/// <summary>
/// a type which extends a given graphql object.
/// </summary>
[TvtFactoryAttribute.IsClass, TvtFactoryAttribute.IsDerivedFrom(typeof(ObjectTypeExtension))]
public record ClassTypeExtensionType(Type Value) : TypeExtensionType(Value);

/// <summary>
/// extension methods for the <seealso cref="IRequestExecutorBuilder"/>
/// </summary>
public static class RequestExecutorBuilderHelper
{

    /// <summary>
    /// <b>WARNING!</b> This method must be called after <b>ALL</b> DataProvider have been registered!
    /// </summary>
    public static IRequestExecutorBuilder AddTypeExtensions(
        this IRequestExecutorBuilder builder, ITypeCache typeCache)
    {
        foreach (var type in typeCache.All<TypeExtensionType>())
            builder.AddTypeExtension(type.Value);
        return builder;
    }

}