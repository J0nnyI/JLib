using System.Collections.Concurrent;
using System.Reflection;

namespace JLib.Helper;

/// <summary>
/// A cache for attributes of any kind to prevent re-evaluating them during each reflection call
/// </summary>
public interface ICustomTypeAttributeCache
{
    /// <summary>
    /// gets all attributes of the specified type from the member
    /// </summary>
    IReadOnlyCollection<TAttribute> GetCustomAttributes<TAttribute>(MemberInfo type, bool inherit = true)
        where TAttribute : Attribute;

    /// <summary>
    /// gets all attributes of the specified type from the member
    /// <br/>throws an <see cref="ArgumentException"/> if <paramref name="attributeType"/> is not an <see cref="Attribute"/>
    /// </summary>
    IReadOnlyCollection<Attribute> GetCustomAttributes(MemberInfo type, Type attributeType, bool inherit = true);

    /// <summary>
    /// gets the single attribute of the specified type from the member.
    /// <br/>throws an <see cref="AmbiguousMatchException"/> if multiple attributes are found
    /// <br/>throws an <see cref="InvalidOperationException"/> if no attribute is found
    /// </summary>
    TAttribute GetCustomAttribute<TAttribute>(MemberInfo type, bool inherit = true) 
        where TAttribute : Attribute;

    /// <summary>
    /// gets the single attribute of the specified type from the member.
    /// <br/>throws an <see cref="AmbiguousMatchException"/> if multiple attributes are found
    /// <br/>throws an <see cref="InvalidOperationException"/> if no attribute is found
    /// <br/>throws an <see cref="ArgumentException"/> if <paramref name="attributeType"/> is not an <see cref="Attribute"/>
    /// </summary>
    Attribute GetCustomAttribute(MemberInfo type, Type attributeType, bool inherit = true);

    /// <summary>
    /// Checks if the specified attribute is defined on the member
    /// </summary>
    bool IsDefined<TAttribute>(MemberInfo type, bool inherit = true)
        where TAttribute : Attribute;

    /// <summary>
    /// Checks if the specified attribute is defined on the member
    /// <br/>throws an <see cref="ArgumentException"/> if <paramref name="attributeType"/> is not an <see cref="Attribute"/>
    /// </summary>
    bool IsDefined(MemberInfo type, Type attributeType, bool inherit = true);

    /// <summary>
    /// clears the cache
    /// </summary>
    void Clear();

    /// <summary>
    /// clears the cache for the specified attribute type
    /// <br/>throws an <see cref="ArgumentException"/> if <paramref name="attributeType"/> is not an <see cref="Attribute"/>
    /// </summary>
    void Clear(Type attributeType);

}

/// <summary>
/// <inheritdoc cref="ICustomTypeAttributeCache"/>
/// </summary>
public class CustomTypeAttributeCache() : ICustomTypeAttributeCache
{
    
    private record AttributeCacheKey(MemberInfo Type, Type AttributeType, bool Inherit);

    private readonly ConcurrentDictionary<AttributeCacheKey, IReadOnlyCollection<Attribute> /*actually IReadonlyCollection<TAttribute>*/> _attributeCache = [];

    #region attribute factory
    private static readonly MethodInfo GetAttributesToBeCachedMi = typeof(CustomTypeAttributeCache)
                                                                   .GetMethods(
                                                                       BindingFlags.Instance | BindingFlags.NonPublic
                                                                   )
                                                                   .SingleOrDefault(
                                                                       x => x is
                                                                       {
                                                                           Name: nameof(FetchAttributesToBeCached),
                                                                           IsGenericMethod:true,
                                                                       } && x.GetParameters() is [ {ParameterType:{Name:nameof(AttributeCacheKey)}} ]
                                                                       && x.GetGenericArguments() is [ _ ]
                                                                   ) ?? throw new Exception($"{nameof(CustomTypeAttributeCache)}.{nameof(FetchAttributesToBeCached)} method not found");
    private IReadOnlyCollection<Attribute> FetchAttributesToBeCached(AttributeCacheKey cacheKey)
        => GetAttributesToBeCachedMi
               .MakeGenericMethod(cacheKey.AttributeType)
               .Invoke(this, [cacheKey])
               as IReadOnlyCollection<Attribute>
        ?? throw new Exception($"returned collection was not convertible to {typeof(IReadOnlyCollection<Attribute>).FullName()}");
    /// <summary>
    /// referenced by <see cref="GetAttributesToBeCachedMi"/>
    /// </summary>
    private IReadOnlyCollection<TAttribute> FetchAttributesToBeCached<TAttribute>(AttributeCacheKey cacheKey)
        where TAttribute : Attribute
        => cacheKey.Type
                   .GetCustomAttributes(cacheKey.AttributeType, cacheKey.Inherit)
                   .Cast<TAttribute>()
                   .ToReadOnlyCollection();

    #endregion

    private void AttributeGuard(Type attributeType)
    {
        if(!attributeType.IsAssignableTo(typeof(Attribute)))
            throw new ArgumentException($"{attributeType.FullName()} is not assignable to {typeof(Attribute).FullName()}");
    }

    private T SingleResultGuard<T>(IReadOnlyCollection<T> result, MemberInfo type, Type attributeType)
        => result.Count switch
        {
            > 1 => throw new AmbiguousMatchException(
                $"{type.Name} has more than one ({result.Count}) attributes of type {attributeType.FullName()}"
            ),
            <= 0 => throw new InvalidOperationException(
                $"{type.Name} has no attributes of type {attributeType.FullName()}"
            ),
            _ => result.Single()
        };

    /// <inheritdoc />
    public IReadOnlyCollection<TAttribute> GetCustomAttributes<TAttribute>(MemberInfo type, bool inherit = true)
        where TAttribute : Attribute
    {
        var key = new AttributeCacheKey(type, typeof(TAttribute), inherit);
        return _attributeCache.GetValueOrAdd(key, FetchAttributesToBeCached<TAttribute>)
            as IReadOnlyCollection<TAttribute> 
            ?? throw new Exception($"returned collection was not convertible to {typeof(IReadOnlyCollection<TAttribute>).FullName()}");
    }
    /// <inheritdoc />
    public IReadOnlyCollection<Attribute> GetCustomAttributes(MemberInfo type, Type attributeType, bool inherit = true)
    {
        AttributeGuard(attributeType);
        var key = new AttributeCacheKey(type, attributeType, inherit);
        return _attributeCache.GetValueOrAdd(key, FetchAttributesToBeCached);
    }

    /// <inheritdoc />
    public TAttribute GetCustomAttribute<TAttribute>(MemberInfo type, bool inherit = true) 
        where TAttribute : Attribute
        => SingleResultGuard(
            GetCustomAttributes<TAttribute>(type, inherit),
            type,
            typeof(TAttribute)
        );

    /// <inheritdoc />
    public Attribute GetCustomAttribute(MemberInfo type, Type attributeType, bool inherit = true)
        => SingleResultGuard(
            GetCustomAttributes(type,attributeType, inherit),
            type,
            attributeType
            );

    /// <inheritdoc />
    public bool IsDefined<TAttribute>(MemberInfo type, bool inherit = true)
        where TAttribute : Attribute
        => IsDefined(type, typeof(TAttribute), inherit);

    /// <inheritdoc />
    public bool IsDefined(MemberInfo type, Type attributeType, bool inherit = true)
    {
        AttributeGuard(attributeType);
        var key = new AttributeCacheKey(type, attributeType, inherit);

        return _attributeCache.TryGetValue(key, out var attributes)
            ? attributes.Count > 0
            : Attribute.IsDefined(type, attributeType, inherit);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _attributeCache.Clear();
    }
    /// <inheritdoc />
    public void Clear(Type attributeType)
    {
        AttributeGuard(attributeType);
        _attributeCache.RemoveWhere(x => x.Key.AttributeType == attributeType);
    }
}