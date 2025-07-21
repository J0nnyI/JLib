using System.Linq.Expressions;
using System.Reflection;
using JLib.ValueTypes;
using static JLib.Reflection.TvtFactoryAttribute;

using ValueType = JLib.ValueTypes.ValueType;

namespace JLib.Reflection;

/// <summary>
/// <see cref="ValueType{T}"/> for <see cref="Type"/>s
/// </summary>
/// <param name="Value"></param>
[DerivedFromAny(typeof(ValueType<>))]
public record ValueTypeType(Type Value) : TypeValueType(Value), IValidatedType
{
    /// <summary>
    /// the TypeArgument of <see cref="ValueType{T}"/>
    /// </summary>
    public Type NativeType=> ValueType.ReflectionResolver.FindNativeType(Value);

    /// <summary>
    /// the constructor which will be used to create the <see cref="ValueType"/>
    /// </summary>
    public ConstructorInfo DefaultConstructor => ValueType.ReflectionResolver.FindConstructor(Value);

    /// <summary>
    /// <b>is to be considered internal.</b><br/>
    /// use <see cref="ValueType.Create"/> or, if direct expression access is required, <see cref="ValueType.FactoryExpressions"/> 
    /// </summary>
    /// <param name="value"></param>
    /// <returns>an expression which will create a new <see cref="ValueType"/> from the given <paramref name="value"/> which does not contain any null checks or other safeguards.</returns>
    public Expression ConstructorExpression(Expression value) => ValueType.ReflectionResolver.CreateValueTypeExpression(Value, value);
    
    void IValidatedType.Validate(ITypeCache cache, IValidationContext<Type> value)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (NativeType is null)
            value.Fail("the NativeType could not be found");

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (DefaultConstructor is null)
            value.Fail("the Constructor could not be found");

        if(Value is { IsGenericType: true, IsAbstract: false })// this is necessary for the ctor call and the factories to work
            value.Fail("the ValueTypeType must be abstract if it is generic");
    }
}