using System.Reflection;
using JLib.Exceptions;
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
    /// Limits the decorated <see cref="TypeValueType"/> to <see cref="Type"/>es decorated with the given <paramref name="attributeType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasAttributeAttribute(Type attributeType) : TvtFactoryAttribute
    {
        public Type AttributeType { get; } = attributeType;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.HasCustomAttribute(AttributeType);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotAbstractAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.IsAbstract;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BeGenericAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsGenericType;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotGenericAttribute : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.IsGenericType;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DerivedFromAnyAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type = type;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsDerivedFromAny(Type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsAssignableToAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type1)
            => type1.IsAssignableTo(Type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type1)
            => type1.IsAssignableTo(Type) && type1 != Type;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;


        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Implements(Type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAnyAttribute(Type type) : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type1)
            => type1.ImplementsAny(type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsNoneAttribute(Type type) : TvtFactoryAttribute
    {
        public Type Type { get; } = type;

        /// <inheritdoc />
        public override bool Filter(Type type1)
            => !type1.ImplementsAny(Type);
    }
#if NET7_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsAssignableToAttribute<T> : TvtFactoryAttribute
        where T : class
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsAssignableTo<T>();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAttribute<T> : TvtFactoryAttribute
        where T : class
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsAssignableTo<T>() && type != typeof(T);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DerivedFromAnyAttribute<T> : TvtFactoryAttribute
        where T : class
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.IsDerivedFromAny<T>();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsNotDerivedFromAny<T> : TvtFactoryAttribute
        where T : class
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.IsDerivedFromAny<T>();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsNotThisTvtAttribute<TTvt> : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => typeof(TTvt).GetCustomAttributes()
                    .OfType<TvtFactoryAttribute>()
                    .All(a => a.Filter(type))
                is false;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsAttribute<T> : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Implements<T>();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsNot<T> : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.Implements<T>();
    }

    /// <summary>
    /// Checks, whether the type implements the given interface, ignoring its type arguments
    /// </summary>
    /// <typeparam name="T">The type to check for, type arguments will be ignored</typeparam>
    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsAnyAttribute<T> : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.ImplementsAny<T>();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsNoneAttribute<T> : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => !type.ImplementsAny<T>();
    }
#endif
}

public abstract class UnrecommendedTvtFactoryAttribute : TvtFactoryAttribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HasNameSuffixAttribute(string suffix) : TvtFactoryAttribute
    {
        public string Suffix { get; } = suffix;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Name.EndsWith(Suffix, StringComparison.Ordinal);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HasNamePrefixAttribute(string prefix) : TvtFactoryAttribute
    {
        public string Prefix { get; } = prefix;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Name.StartsWith(prefix, StringComparison.Ordinal);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsInNamespace(string @namespace) : TvtFactoryAttribute
    {
        public string Namespace = @namespace;

        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Namespace?.StartsWith(Namespace, StringComparison.Ordinal) is true;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HasNonamespace : TvtFactoryAttribute
    {
        /// <inheritdoc />
        public override bool Filter(Type type)
            => type.Namespace is null;
    }
}