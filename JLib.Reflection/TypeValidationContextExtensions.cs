using System.Reflection;

using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

/// <summary>
/// Extension methods for validating <see cref="Type"/>s
/// </summary>
public static class TypeValidationContextExtensions
{
    /// <summary>
    /// Validates all properties of the <see cref="Type"/> which match the given <paramref name="filter"/>
    /// </summary>
    public static IValidationContext<Type> ValidateProperties(this IValidationContext<Type> context, Func<PropertyInfo, bool> filter, Action<ValidationContext<PropertyInfo>> validator)
    {
        foreach (var property in context.Value.GetProperties().Where(filter))
        {
            var val = new ValidationContext<PropertyInfo>(property, context.TargetType);
            validator(val);
            context.AddSubValidators(val);
        }
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be generic
    /// </summary>
    public static IValidationContext<Type> ShouldBeGeneric(this IValidationContext<Type> context, string? hint = null)
    {
        if (!context.Value.IsGenericType)
            context.AddError(string.Join(Environment.NewLine, "Must be Generic", hint));
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be static
    /// </summary>
    public static IValidationContext<Type> ShouldBeStatic(this IValidationContext<Type> context, string? hint = null)
    {
        if (!context.Value.IsStatic())
            context.AddError(string.Join(Environment.NewLine, "Must be Static", hint));
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be sealed
    /// </summary>
    public static IValidationContext<Type> ShouldBeSealed(this IValidationContext<Type> context, string? hint = null)
    {
        if (!context.Value.IsSealed)
            context.AddError(string.Join(Environment.NewLine, "Must be Sealed", hint));
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be a generic type
    /// </summary>
    public static IValidationContext<Type> ShouldNotBeGeneric(this IValidationContext<Type> context, string? hint = null)
    {
        if (context.Value.IsGenericType)
            context.AddError(string.Join(Environment.NewLine, "Must not be Generic", hint));
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be a generic type and have exactly <paramref name="argumentCount"/> type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldHaveNTypeArguments(this IValidationContext<Type> context, int argumentCount)
    {
        context.ShouldBeGeneric();

        if (context.Value.GenericTypeArguments.Length != argumentCount)
            context.AddError(
                $"It must have exactly {argumentCount} type arguments but got {context.Value.GenericTypeArguments.Length}");
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to implement <typeparamref name="TInterface"/> ignoring all of its type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldImplementAny<TInterface>(this IValidationContext<Type> context, string? hint = null)
        => context.ShouldImplementAny(typeof(TInterface), hint);

    /// <summary>
    /// Expects the <see cref="Type"/> to implement <paramref name="tInterface"/> ignoring all of its type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldImplementAny(this IValidationContext<Type> context, Type tInterface, string? hint = null)
    {
        if (!context.Value.ImplementsAny(tInterface))
            context.AddError($"Should implement any {tInterface.TryGetGenericTypeDefinition().FullName(true)}",
                hint);
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to implement <typeparamref name="TInterface"/> with the given type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldImplement<TInterface>(this IValidationContext<Type> context, string? hint = null)
        => context.ShouldImplement(typeof(TInterface), hint);

    /// <summary>
    /// Expects the <see cref="Type"/> to implement <paramref name="tInterface"/> with the given type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldImplement(this IValidationContext<Type> context, Type tInterface, string? hint = null)
    {
        if (!context.Value.ImplementsAny(tInterface))
            context.AddError($"Should implement {tInterface.FullName(true)}", hint);
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> not to implement <typeparamref name="TInterface"/> ignoring all of its type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldNotImplementAny<TInterface>(this IValidationContext<Type> context, string? hint = null)
        => context.ShouldNotImplementAny(typeof(TInterface), hint);

    /// <summary>
    /// Expects the <see cref="Type"/> not to implement <paramref name="tInterface"/> ignoring all of its type arguments
    /// </summary>
    public static IValidationContext<Type> ShouldNotImplementAny(this IValidationContext<Type> context, Type tInterface, string? hint = null)
    {
        if (context.Value.ImplementsAny(tInterface))
            context.AddError($"Should not implement {tInterface.TryGetGenericTypeDefinition().FullName(true)}",
                hint);
        return context;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be decorated with <typeparamref name="TAttribute"/>
    /// </summary>
    public static IValidationContext<Type> ShouldHaveAttribute<TAttribute>(this IValidationContext<Type> context, string? hint = null)
        where TAttribute : Attribute
        => context.ShouldHaveAttribute(typeof(TAttribute), hint);

    /// <summary>
    /// Expects the <see cref="Type"/> to be decorated with <paramref name="tAttribute"/>
    /// </summary>
    public static IValidationContext<Type> ShouldHaveAttribute(this IValidationContext<Type> context, Type tAttribute, string? hint = null)
    {
        if (!context.Value.HasCustomAttribute(tAttribute))
            context.AddError($"Should have {tAttribute.FullName(true)}", hint);
        return context;
    }

    /// <summary>
    /// expects the <see cref="MemberInfo.Name"/> to <see cref="string.Equals(string?,StringComparison)"/> <paramref name="name"/> with the <paramref name="comparisonType"/>
    /// </summary>
    public static IValidationContext<Type> ShouldHaveName(this IValidationContext<Type> context, string name, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (context.Value.Name.Equals(name, comparisonType))
            context.AddError($"must have the name '{name}'");
        return context;
    }

    /// <summary>
    /// expects the <see cref="MemberInfo.Name"/> to end with the given <paramref name="nameSuffix"/>
    /// </summary>
    public static IValidationContext<Type> ShouldHaveNameSuffix(this IValidationContext<Type> context, string nameSuffix)
    {
        if (!context.Value.Name.EndsWith(nameSuffix))
            context.AddError($"must have the nameSuffix '{nameSuffix}'");
        return context;
    }
}