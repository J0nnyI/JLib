using System.Reflection;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// classes with this given attribute will not be ignored by the typeCache
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreInCache : Attribute
{
}

/// <summary>
/// used by the <seealso cref="TvtFactoryAttribute"/> to apply a custom factory to this value type
/// </summary>
public interface ITypeValueTypeFilter
{
    /// <summary>
    /// Determines whether the given <paramref name="type"/> satisfies the filter condition.<br/>
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns><see langword="true"/>, if the <paramref name="type"/>satisfies the filter condition</returns>
    bool Filter(Type type);
}

/// <summary>
/// Serves as the base class for attributes that define type filtering logic.
/// </summary>
/// <remarks>Derived classes implement the <see cref="Filter"/> method to specify custom filtering criteria for
/// types. These attributes are typically applied to classes to indicate whether they meet certain conditions, such as
/// being an interface, a class, or implementing specific interfaces.</remarks>
public abstract class TvtFactoryAttribute : Attribute
{
    /// <inheritdoc cref="ITypeValueTypeFilter.Filter"/>
    public abstract bool Filter(Type type);

    /// <summary>
    /// Used to resolve type cache conflicts which happen, when two <see cref="TypeValueType"/>s match one <see cref="Type"/>.<br/>
    /// In such cases, the <see cref="TypeValueType"/> with the lowest <see cref="PriorityAttribute.Value"/> will be used.<br/>
    /// If two attributes have the same priority, an exception will be thrown.<br/>
    /// default is <see langword="10_000"/><br/>
    /// this is a crutch until derivation tree based prioritization is added
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PriorityAttribute(int value) : Attribute
    {
        /// <summary>
        /// The Priority of the decorated <see cref="TypeValueType"/>.
        /// </summary>
        public int Value { get; } = value;

        /// <summary>
        /// The Default Priority relative to which derivations might orient themselves.<br/>
        /// </summary>
        public const int DefaultPriority = 10_000;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see langword="interface"/>s
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IsInterfaceAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsInterface;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see langword="class"/>es
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IsClassAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsClass;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see langword="interface"/>s which have the given <paramref name="attributeType"/>.
    /// </summary>
    public class HasInterfaceWithAttributeAttribute(Type attributeType) : TvtFactoryAttribute
    {
        /// <summary>
        /// The <see cref="Attribute"/> <see cref="Type"/> at least one <see langword="interface"/> should be decorated with
        /// </summary>
        public Type AttributeType { get; } = attributeType;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.GetInterfaces().Any(i => i.HasCustomAttribute(AttributeType));
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see langword="interface"/>s which have the given <typeparamref name="T"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasInterfaceWithAttributeAttribute<T>()
        : HasInterfaceWithAttributeAttribute(typeof(T)) where T : Attribute;

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="ReflectionHelper.HasCustomAttribute"/> evaluates to <see langword="true"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasAttributeAttribute(Type attributeType) : TvtFactoryAttribute
    {
        public Type AttributeType { get; } = attributeType;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.HasCustomAttribute(AttributeType);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="ReflectionHelper.HasCustomAttribute"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="ReflectionHelper.HasCustomAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasAttributeAttribute<T>() : HasAttributeAttribute(typeof(T)) where T : Attribute;

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsAbstract"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsAbstract"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NotAbstractAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.IsAbstract;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsGenericType"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsGenericType"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BeGenericAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsGenericType;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsGenericType"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsGenericType"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NotGenericAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.IsGenericType;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.IsDerivedFromAny"/> with <paramref name="type"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.IsDerivedFromAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DerivedFromAnyAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type = type;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsDerivedFromAny(Type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.IsDerivedFromAny"/> with <typeparamref name="T"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.IsDerivedFromAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DerivedFromAnyAttribute<T>() : DerivedFromAnyAttribute(typeof(T));

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsAssignableTo"/> with <paramref name="type"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsAssignableTo"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IsAssignableToAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type1)
            => type1.IsAssignableTo(Type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsAssignableTo"/> with <paramref name="type"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsAssignableTo"/>
    /// </summary>
    public sealed class IsAssignableToAttribute<T>() : IsAssignableToAttribute(typeof(T));

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsAssignableTo"/> with <paramref name="type"/> evaluates to <see langword="true"/> and which are not <paramref name="type"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsAssignableTo"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IsDerivedFromAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type1)
            => type1.IsAssignableTo(Type) && type1 != Type;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="Type.IsAssignableTo"/> with <typeparamref name="T"/> evaluates to <see langword="true"/> and which are not <typeparamref name="T"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="Type.IsAssignableTo"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAttribute<T>() : IsDerivedFromAttribute(typeof(T));

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.Implements"/> with <typeparamref name="T"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.Implements"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ImplementsAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;


        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Implements(Type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.Implements"/> with <typeparamref name="T"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.Implements"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAttribute<T>() : ImplementsAttribute(typeof(T));

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.ImplementsAny"/> with <paramref name="type"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.ImplementsAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ImplementsAnyAttribute(Type type) : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type1)
            => type1.ImplementsAny(type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.ImplementsAny"/> with <typeparamref name="T"/> evaluates to <see langword="true"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.ImplementsAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAnyAttribute<T>() : ImplementsAnyAttribute(typeof(T));

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.Implements"/> with <typeparamref name="T"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.Implements"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ImplementsNotAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;


        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Implements(Type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.Implements"/> with <typeparamref name="T"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.Implements"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsNotAttribute<T>() : ImplementsNotAttribute(typeof(T));

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.ImplementsAny"/> with <typeparamref name="T"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.ImplementsAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ImplementsNoneAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type1)
            => !type1.ImplementsAny(Type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.ImplementsAny"/> with <typeparamref name="T"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.ImplementsAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsNoneAttribute<T>() : ImplementsNoneAttribute(typeof(T))
    {
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.IsDerivedFromAny"/> with <paramref name="type"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.IsDerivedFromAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IsNotDerivedFromAny(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.IsDerivedFromAny(type);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.IsDerivedFromAny"/> with <typeparamref name="T"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.IsDerivedFromAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsNotDerivedFromAny<T>() : IsNotDerivedFromAny(typeof(T))
        where T : class;


    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which would not be matched by the given <paramref name="typeValueType"/>'s <see cref="TvtFactoryAttribute"/>s.
    /// </summary>
    public class IsNotThisTvtAttribute(Type typeValueType) : TvtFactoryAttribute()
    {
        public Type TypeValueType { get; } = typeValueType;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => TypeValueType.GetCustomAttributes()
                    .OfType<TvtFactoryAttribute>()
                    .All(a => a.Filter(type))
                is false;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which would not be matched by the given <typeparamref name="TTvt"/>'s <see cref="TvtFactoryAttribute"/>s.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsNotThisTvtAttribute<TTvt>() : IsNotThisTvtAttribute(typeof(TTvt));



    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s of which <see cref="TypeHelper.IsDerivedFromAny"/> with <paramref name="type"/> evaluates to <see langword="false"/><br/>
    /// -----------------------------------------------------------------------------------------------<br/>
    /// <inheritdoc cref="TypeHelper.IsDerivedFromAny"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IsDefinedInType(Type containerType, bool includeSubTypes) : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type) 
            => type.IsDefinedInType(containerType, includeSubTypes);
    }
}

/// <summary>
/// Serves as the base class for attributes that define type filtering logic for <see cref="TypeValueType"/>s that are not recommended to be used.<br/>
/// Reason: They result in magic names and/or namespaces, which behave less predictable and implement antipattern.<br/>
/// </summary>
public abstract class UnrecommendedTvtFactoryAttribute : TvtFactoryAttribute
{
    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which end with <paramref name="suffix"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNameSuffixAttribute(string suffix) : TvtFactoryAttribute
    {
        public string Suffix { get; } = suffix;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Name.EndsWith(Suffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which do not end with <paramref name="suffix"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNotNameSuffixAttribute(string suffix) : TvtFactoryAttribute
    {
        public string Suffix { get; } = suffix;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Name.EndsWith(Suffix, StringComparison.Ordinal) is false;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which start with <paramref name="prefix"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNamePrefixAttribute(string prefix) : TvtFactoryAttribute
    {
        public string Prefix { get; } = prefix;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Name.StartsWith(prefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which do not start with <paramref name="prefix"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNotNamePrefixAttribute(string prefix) : TvtFactoryAttribute
    {
        public string Prefix { get; } = prefix;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Name.StartsWith(Prefix, StringComparison.Ordinal) is false;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which are defined in the given <paramref name="namespace"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IsDefinedInNamespace(string @namespace, bool includeSubNamespaces = false) : TvtFactoryAttribute
    {
        public string Namespace { get; } = @namespace;
        public bool IncludeSubNamespaces { get; } = includeSubNamespaces;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsDefinedInNamespace(Namespace, IncludeSubNamespaces);
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which are not defined in the given <paramref name="namespace"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IsNotDefinedInNamespace(string @namespace, bool includeSubNamespaces = false) : TvtFactoryAttribute
    {
        public string Namespace { get; } = @namespace;
        public bool IncludeSubNamespaces { get; } = includeSubNamespaces;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsDefinedInNamespace(Namespace, IncludeSubNamespaces) is false;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which are not defined in any namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNoNamespace : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Namespace is null;
    }

    /// <summary>
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>s which are defined in any namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNamespace : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Namespace is not null;
    }
}