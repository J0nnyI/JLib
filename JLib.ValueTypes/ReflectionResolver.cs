using System.Linq.Expressions;
using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;
partial class ValueType
{
    /// <summary>
    /// Contains the standard methods to resolve <see cref="ValueType{T}"/> members.<br/>
    /// They should be considered internal and may be updated later.
    /// </summary>
    public static class ReflectionResolver
    {

        /// <summary>
        /// Finds the <see cref="ValueType{T}.Value"/>'s <see cref="Type"/>
        /// </summary>
        public static Type FindNativeType(Type tValueType)
            => tValueType.GetAnyBaseType<ValueType<Ignored>>()?.GenericTypeArguments.First()
               ?? throw new InvalidSetupException($"{tValueType.FullName()} is not a valueType.");

        /// <summary>
        /// Finds the Constructor for the given <see cref="TVt"/>, even if it is not public.
        /// </summary>
        /// <exception cref="InvalidSetupException"></exception>
        public static ConstructorInfo FindConstructor<TVt, TV>()
            where TVt : ValueType<TV>
            => FindConstructor(typeof(TVt));

        /// <summary>
        /// Finds the Constructor for the given <paramref name="tValueType"/>, even if it is not public.
        /// </summary>
        /// <exception cref="InvalidSetupException"></exception>
        public static ConstructorInfo FindConstructor(Type tValueType)
        {
            try
            {
                var tNative = FindNativeType(tValueType);
                return tValueType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, new[] { tNative })
                    ?? throw new InvalidSetupException($"could not find ctor of valueType {tValueType.FullName()}");
            }
            catch (InvalidSetupException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidSetupException($"ctor discovery of valueType {tValueType.FullName()} failed", e);
            }
        }

        /// <summary>
        /// Finds the Constructor for the given <see cref="TVt"/>, even if it is not public.
        /// </summary>
        /// <exception cref="InvalidSetupException"></exception>
        public static Expression CreateValueTypeExpression<TVt, TV>(Expression value)
            where TVt : ValueType<TV>
            => CreateValueTypeExpression(typeof(TVt), value);

        /// <summary>
        /// Creates an <see cref="Expression"/> which creates the given valueType.
        /// </summary>
        public static Expression CreateValueTypeExpression(Type tValueType, Expression value)
        {
            // the reason the methods are split like this is to enable factory support later on without any issues
            var ctor = FindConstructor(tValueType);
            return Expression.New(ctor, value);
        }
    }
}
