using JLib.Exceptions;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// indicates, that the <see cref="TypeCache"/> threw an exception
/// </summary>
public abstract class TypeCacheException : JLibException
{
    internal TypeCacheException(string message) : base(message)
    {
    }

    internal TypeCacheException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="GivenType"/> could not be resolved as <see cref="RequestedTypeValueType"/>
/// </summary>
public abstract class TypeResolverException : TypeCacheException
{
    /// <summary>
    /// The <see cref="TypeValueType"/> which was requested
    /// </summary>
    public Type RequestedTypeValueType { get; }
    /// <summary>
    /// The Type which was requested to be resolved
    /// </summary>
    public Type? GivenType { get; }

    internal TypeResolverException(Type requestedTypeValueType, Type? givenType, string message) : base(message)
    {
        RequestedTypeValueType = requestedTypeValueType;
        Data[nameof(RequestedTypeValueType)] = requestedTypeValueType;
        GivenType = givenType;
        Data[nameof(GivenType)] = givenType;
    }
}

/// <summary>
/// Indicates, that the <see cref="TypeNotFoundException.GivenType"/> is not registered in the <see cref="ITypeCache"/> under any <see cref="TypeValueType"/>
/// </summary>
public abstract class TypeNotFoundException : TypeResolverException
{
    internal TypeNotFoundException(Type requestedTypeValueType, Type? givenType)
        : base(requestedTypeValueType, givenType,
            $"The TypeCache does not contain an instance of {givenType?.FullName(true)}. It was requested as {requestedTypeValueType.FullName(true)}")
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="ITypeCache"/> does not contain any return any <see cref="TypeValueType"/>s which are assignable to <typeparamref name="TRequestedTypeValueType"/> and are either the <see cref="TypeResolverException.GivenType"/> or satisfy a given filter.
/// </summary>
public sealed class TypeNotFoundException<TRequestedTypeValueType> : TypeNotFoundException
    where TRequestedTypeValueType : ITypeValueType
{
    /// <summary>
    /// Indicates, that the filter did not return any <see cref="TypeValueType"/>s in the <see cref="ITypeCache"/> under any <typeparamref name="TRequestedTypeValueType"/>
    /// </summary>
    internal TypeNotFoundException() : base(typeof(TRequestedTypeValueType), null)
    {
    }
    /// <summary>
    /// Indicates, that the filter did not return any <see cref="TypeValueType"/>s in the <see cref="ITypeCache"/> under any <typeparamref name="TRequestedTypeValueType"/>
    /// </summary>
    internal TypeNotFoundException(Type givenType) : base(typeof(TRequestedTypeValueType), givenType)
    {
    }
}

/// <summary>
/// Indicates, that the filter expression used to retrieve a single <see cref="TypeResolverException.RequestedTypeValueType"/> from the <see cref="ITypeCache"/> is true for more than one <see cref="TypeValueType"/>
/// </summary>
public abstract class NotUniqueTypeFilterException : TypeResolverException
{
    internal NotUniqueTypeFilterException(Type requestedTypeValueType) : base(requestedTypeValueType, null, $"The TypeCache does contain multiple valueTypes assignable to {requestedTypeValueType.FullName()} which satisfy the given condition")
    {
    }
}
/// <summary>
/// Indicates, that the filter expression used to retrieve a single <see cref="TypeResolverException.RequestedTypeValueType"/> from the <see cref="ITypeCache"/> is true for more than one <see cref="TypeValueType"/>
/// </summary>
public sealed class NotUniqueTypeFilterException<TRequestedTypeValueType> : NotUniqueTypeFilterException
    where TRequestedTypeValueType : ITypeValueType
{
    internal NotUniqueTypeFilterException() : base(typeof(TRequestedTypeValueType))
    {
    }
}

/// <summary>
/// Thrown, when the <see cref="ITypeCache"/> does not contain the given <see cref="TypeResolverException.RequestedTypeValueType"/>.<br/>
/// This points to an incomplete <see cref="ITypePackage"/> passed to the <see cref="ITypeCache"/>
/// </summary>
public abstract class UnknownTypeValueTypeException : TypeResolverException
{
    internal UnknownTypeValueTypeException(Type requestedTypeValueType, Type givenType)
        : base(requestedTypeValueType, givenType,
            $"The TypePackage passed to the TypeCache did not contain the requested TypeValueType {requestedTypeValueType.FullName(true)} while resolving {givenType.FullName(true)}")
    {
    }
}

/// <summary>
/// Thrown, when the <see cref="ITypeCache"/> does not contain <typeparamref cref="TRequestedTypeValueType"/>.<br/>
/// This points to an incomplete <see cref="ITypePackage"/> passed to the <see cref="ITypeCache"/>
/// </summary>
/// <typeparam name="TRequestedTypeValueType"></typeparam>
public sealed class UnknownTypeValueTypeException<TRequestedTypeValueType> : UnknownTypeValueTypeException
    where TRequestedTypeValueType : ITypeValueType
{
    internal UnknownTypeValueTypeException(Type givenType) : base(typeof(TRequestedTypeValueType), givenType)
    {
    }
}
/// <summary>
/// Indicates, that the <see cref="ITypePackage"/> passed to the <see cref="ITypeCache"/> did not contain the <see cref="TypeResolverException.GivenType"/>
/// </summary>
public abstract class UnknownTypeException : TypeResolverException
{
    internal UnknownTypeException(Type requestedTypeValueType, Type givenType)
        : base(requestedTypeValueType, givenType,
            $"The TypePackage passed to the TypeCache did not contain {givenType.FullName(true)} as {requestedTypeValueType.FullName(true)}")
    {
    }
}
/// <summary>
/// Indicates, that the <see cref="ITypePackage"/> passed to the <see cref="ITypeCache"/> did not contain the <see cref="TypeResolverException.GivenType"/>
/// </summary>
public sealed class UnknownTypeException<TRequestedTypeValueType> : UnknownTypeException
    where TRequestedTypeValueType : ITypeValueType
{
    internal UnknownTypeException(Type givenType) : base(typeof(TRequestedTypeValueType), givenType)
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="TypeResolverException.GivenType"/> was found in the <see cref="ITypeCache"/> but it was not associated with the expected <see cref="TypeResolverException.RequestedTypeValueType"/> but instead with <see cref="ActualTypeValueType"/>, which are not assignable
/// </summary>
public abstract class TypeValueTypeMismatchException : TypeResolverException
{
    /// <summary>
    /// The Actual <see cref="TypeValueType"/> under which the <see cref="TypeResolverException.GivenType"/> is registered in the <see cref="ITypeCache"/>
    /// </summary>
    public Type ActualTypeValueType { get; }

    internal TypeValueTypeMismatchException(Type requestedTypeValueType, Type actualTypeValueType, Type givenType)
        : base(requestedTypeValueType, givenType,
            $"{givenType.FullName(true)} was requested as {requestedTypeValueType.FullName(true)} which is not assignable to its actual {nameof(TypeValueType)} of {actualTypeValueType.FullName(true)}")
    {
        ActualTypeValueType = actualTypeValueType;
        Data[nameof(ActualTypeValueType)] = actualTypeValueType;
    }
}
/// <summary>
/// Indicates, that the <see cref="TypeResolverException.GivenType"/> was found in the <see cref="ITypeCache"/> but it was not associated with the expected <typeparamref cref="TRequestedTypeValueType"/> but instead with <see cref="TypeValueTypeMismatchException.ActualTypeValueType"/>, which are not assignable
/// </summary>
public sealed class TypeValueTypeMismatchException<TRequestedTypeValueType> : TypeValueTypeMismatchException
    where TRequestedTypeValueType : ITypeValueType
{
    internal TypeValueTypeMismatchException(Type actualTypeValueType, Type givenType)
        : base(typeof(TRequestedTypeValueType), actualTypeValueType, givenType)
    {
    }
}