using System.Linq.Expressions;
using System.Reflection;

namespace JLib.Helper;

/// <summary>
/// contains extension methods for working with <see cref="Expression"/>s
/// </summary>
public static class ExpressionHelper
{
    /// <summary>
    /// Converts the specified expression to a nullable expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A nullable expression.</returns>
    public static Expression<Func<TSource, TKey?>> ToNullable<TSource, TKey>(
        this Expression<Func<TSource, TKey>> expression)
        where TKey : struct
    {
        var param = Expression.Parameter(typeof(TSource), expression.Parameters[0].Name);
        var body = Expression.Convert(expression.Body, typeof(TKey?));
        return Expression.Lambda<Func<TSource, TKey?>>(body, param);
    }

    /// <summary>
    /// Gets the property information from the specified property lambda expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="propertyLambda">The property lambda expression.</param>
    /// <returns>The property information.</returns>
    public static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> propertyLambda)
    {
        Type type = typeof(TSource);

        MemberExpression member = propertyLambda.Body switch
        {
            MemberExpression directMember => directMember,
            UnaryExpression { Operand: MemberExpression indirectMember } => indirectMember,
            _ => throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.")
        };

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

        if (propInfo.ReflectedType is null)
            throw new ArgumentException($"Expression '{propertyLambda}' has no ReflectedType.");

        if (type != propInfo.ReflectedType && (propInfo.ReflectedType.IsInterface &&
                                               !type.Implements(propInfo.ReflectedType))
                                           && !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException(
                $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

        return propInfo;
    }

    /// <summary>
    /// Replaces all calls to the specified <paramref name="method"/> with the specified <paramref name="replacementExpression"/> in the given <paramref name="inputExpression"/> of type <typeparamref name="T"/> using an <see cref="ExpressionVisitor"/>.<br/>
    ///     <list type="bullet">
    ///         <item>must be a <see cref="LambdaExpression"/></item>
    ///         <item>all parameter types must match those of <paramref name="method"/></item>
    ///         <item>return type must match that of <paramref name="method"/></item>
    ///         <item>return type must match that of <paramref name="method"/></item>
    ///     </list>
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="inputExpression"/> and the Return Value.</typeparam>
    /// <param name="inputExpression">The expression to be edited.</param>
    /// <param name="method">The method to be replaced.</param>
    /// <param name="replacementExpression">The <see cref="LambdaExpression"/> to replace the <paramref name="method"/> with. <br/></param> 
    /// <returns>An expression with all occurrences of the <paramref name="method"/> replaced with the <paramref name="replacementExpression"/> expression.</returns>
    public static Expression<T> ReplaceMethod<T>(this Expression<T> inputExpression, MethodInfo method,
        LambdaExpression replacementExpression)
    {
        var visitor = new ReplaceExpressionVisitor(method, replacementExpression);
        var ex = visitor.Visit(inputExpression);
        return (Expression<T>)ex;
    }

    #region expression visitors

    /// <summary>
    /// replaces all occurrences of method calls to <see cref="_replace"/> with the given expression. <see cref="_with"/><br/>
    /// Uses the <see cref="ParameterVisitor"/> to replace the <see cref="LambdaExpression.Parameters"/> of the <see cref="_with"/> Expression with the <see cref="MethodCallExpression.Arguments"/> values of the specific <see cref="_replace"/> insatnce.
    /// </summary>
    private sealed class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _replace;
        private readonly Expression _with;

        public ReplaceExpressionVisitor(MethodInfo replace, Expression with)
        {
            _replace = replace;
            _with = with;

            switch (with)
            {
                case LambdaExpression lambda:
                    // check parameter types
                    var parameterPairs = lambda.Parameters.Zip(replace.GetParameters(),
                        (lambdaPar, replacePar) => new { lambdaPar, replacePar })
                        .ToArray();
                    if (parameterPairs.Any(x =>
                            x.lambdaPar.Type.IsAssignableTo(x.replacePar.ParameterType) == false
                            && !x.replacePar.ParameterType.IsGenericParameter // type parameters are not resolved yet, therefore string can not be assigned to the unknown type T (it is not defined yet)
                            )
                        )
                        throw new ArgumentException($"parameter type mismatch: " +
                                                    $"found: ({string.Join(", ", parameterPairs.Select(x => x.lambdaPar.Type.Name + " " + x.lambdaPar.Name))}) | " +
                                                    $"compare:  {replace.ToInfoString()}");
                    if (replace.ReturnType.IsGenericParameter
                        ? lambda.ReturnType.IsAssignableTo(replace.ReturnType)
                        : lambda.ReturnType != replace.ReturnType)
                        throw new ArgumentException("return type mismatch");
                    break;
                default:
                    throw new NotSupportedException(
                        $"expressions of type {_with.GetType().FullName()} are not supported");
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if ((node.Method.IsGenericMethod ? node.Method.GetGenericMethodDefinition() : node.Method) != _replace)
                return base.VisitMethodCall(node);

            switch (_with)
            {
                case LambdaExpression lambda:
                    // we are still referring to the parameters of the method info. we have to replace all occurrences with the expression argument. since this happens inside the body, another visitor seems to be the best solution.
                    var visitor = new ParameterVisitor(node.Arguments.Zip(lambda.Parameters, (arg, par) => new { arg, par })
                        .ToDictionary(x => x.par, x => x.arg));
                    var body = visitor.Visit(lambda.Body);
                    return body;
                default:
                    throw new NotSupportedException(
                        $"expressions of type {_with.GetType().FullName()} are not supported");
            }
        }
    }

    /// <summary>
    /// replaces all occurrences of a parameter with a given expression.
    /// </summary>
    private sealed class ParameterVisitor : ExpressionVisitor
    {
        private readonly IReadOnlyDictionary<ParameterExpression, Expression> _parameters;

        public ParameterVisitor(IReadOnlyDictionary<ParameterExpression, Expression> parameters)
        {
            _parameters = parameters;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var argument = _parameters.GetValueOrDefault(node);
            return argument ?? base.VisitParameter(node);
        }
    }

    /// <summary>
    /// Provides methods to compare expressions for equivalence.
    /// </summary>
    public static class ExpressionComparer
    {
        /// <summary>
        /// Compares two expressions to determine if they are equivalent.
        /// </summary>
        /// <param name="expr1">The first expression to compare.</param>
        /// <param name="expr2">The second expression to compare.</param>
        /// <param name="failureReason">The reason why the expressions are not equivalent, if they are not.</param>
        /// <returns>True if the expressions are equivalent; otherwise, false.</returns>
        public static bool AreExpressionsEquivalent(Expression expr1, Expression expr2, out string failureReason)
        {
            failureReason = string.Empty;

            if (expr1 == null && expr2 == null)
                return true;
            if (expr1 == null || expr2 == null)
            {
                failureReason = "One of the expressions is null.";
                return false;
            }

            if (expr1.NodeType != expr2.NodeType)
            {
                failureReason = $"Node types are different: {expr1.NodeType} vs {expr2.NodeType}.";
                return false;
            }

            if (expr1.Type != expr2.Type)
            {
                failureReason = $"Expression types are different: {expr1.Type} vs {expr2.Type}.";
                return false;
            }

            switch (expr1.NodeType)
            {
                case ExpressionType.Constant:
                    return AreConstantExpressionsEquivalent((ConstantExpression)expr1, (ConstantExpression)expr2, out failureReason);
                case ExpressionType.Parameter:
                    return AreParameterExpressionsEquivalent((ParameterExpression)expr1, (ParameterExpression)expr2, out failureReason);
                case ExpressionType.Lambda:
                    return AreLambdaExpressionsEquivalent((LambdaExpression)expr1, (LambdaExpression)expr2, out failureReason);
                case ExpressionType.Call:
                    return AreMethodCallExpressionsEquivalent((MethodCallExpression)expr1, (MethodCallExpression)expr2, out failureReason);
                case ExpressionType.Convert:
                    return AreUnaryExpressionsEquivalent((UnaryExpression)expr1, (UnaryExpression)expr2, out failureReason);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return AreBinaryExpressionsEquivalent((BinaryExpression)expr1, (BinaryExpression)expr2, out failureReason);
                case ExpressionType.Conditional:
                    return AreConditionalExpressionsEquivalent((ConditionalExpression)expr1, (ConditionalExpression)expr2, out failureReason);
                case ExpressionType.Invoke:
                    return AreInvocationExpressionsEquivalent((InvocationExpression)expr1, (InvocationExpression)expr2, out failureReason);
                case ExpressionType.New:
                    return AreNewExpressionsEquivalent((NewExpression)expr1, (NewExpression)expr2, out failureReason);
                default:
                    failureReason = $"Unsupported expression type: {expr1.NodeType}.";
                    return false;
            }
        }

        private static bool AreNewExpressionsEquivalent(NewExpression new1, NewExpression new2, out string failureReason)
        {
            failureReason = string.Empty;
            if (new1.Constructor != new2.Constructor)
            {
                failureReason = $"Constructors are different: {new1.Constructor} vs {new2.Constructor}.";
                return false;
            }

            if (new1.Arguments.Count != new2.Arguments.Count)
            {
                failureReason = "Constructor argument counts are different.";
                return false;
            }

            for (int i = 0; i < new1.Arguments.Count; i++)
            {
                if (!AreExpressionsEquivalent(new1.Arguments[i], new2.Arguments[i], out failureReason))
                {
                    failureReason = $"Constructor arguments at index {i} are different: {failureReason}";
                    return false;
                }
            }

            return true;
        }

        private static bool AreConstantExpressionsEquivalent(ConstantExpression c1, ConstantExpression c2, out string failureReason)
        {
            failureReason = string.Empty;
            if (!Equals(c1.Value, c2.Value))
            {
                failureReason = $"Constant values are different: {c1.Value} vs {c2.Value}.";
                return false;
            }
            return true;
        }

        private static bool AreParameterExpressionsEquivalent(ParameterExpression p1, ParameterExpression p2, out string failureReason)
        {
            failureReason = string.Empty;
            if (p1.Name != p2.Name || p1.Type != p2.Type)
            {
                failureReason = $"Parameter expressions are different: {p1.Name} ({p1.Type}) vs {p2.Name} ({p2.Type}).";
                return false;
            }
            return true;
        }

        private static bool AreLambdaExpressionsEquivalent(LambdaExpression lambda1, LambdaExpression lambda2, out string failureReason)
        {
            failureReason = string.Empty;
            if (lambda1.Parameters.Count != lambda2.Parameters.Count)
            {
                failureReason = "Lambda parameter counts are different.";
                return false;
            }

            if (!AreExpressionsEquivalent(lambda1.Body, lambda2.Body, out failureReason))
            {
                failureReason = $"Lambda bodies are different: {failureReason}";
                return false;
            }

            for (int i = 0; i < lambda1.Parameters.Count; i++)
            {
                if (!AreParameterExpressionsEquivalent(lambda1.Parameters[i], lambda2.Parameters[i], out failureReason))
                {
                    failureReason = $"Lambda parameters at index {i} are different: {failureReason}";
                    return false;
                }
            }

            return true;
        }

        private static bool AreMethodCallExpressionsEquivalent(MethodCallExpression call1, MethodCallExpression call2, out string failureReason)
        {
            failureReason = string.Empty;
            if (call1.Method != call2.Method)
            {
                failureReason = $"Method calls are different: {call1.Method} vs {call2.Method}.";
                return false;
            }

            if (!AreExpressionsEquivalent(call1.Object!, call2.Object!, out failureReason)) // Instance method object
            {
                failureReason = $"Method call objects are different: {failureReason}";
                return false;
            }

            if (call1.Arguments.Count != call2.Arguments.Count)
            {
                failureReason = "Method call argument counts are different.";
                return false;
            }

            for (int i = 0; i < call1.Arguments.Count; i++)
            {
                if (!AreExpressionsEquivalent(call1.Arguments[i], call2.Arguments[i], out failureReason))
                {
                    failureReason = $"Method call arguments at index {i} are different: {failureReason}";
                    return false;
                }
            }

            return true;
        }

        private static bool AreUnaryExpressionsEquivalent(UnaryExpression unary1, UnaryExpression unary2, out string failureReason)
        {
            failureReason = string.Empty;
            if (unary1.NodeType != unary2.NodeType)
            {
                failureReason = $"Unary node types are different: {unary1.NodeType} vs {unary2.NodeType}.";
                return false;
            }
            if (unary1.Method != unary2.Method)
            {
                failureReason = $"Unary methods are different: {unary1.Method} vs {unary2.Method}.";
                return false;
            }
            if (!AreExpressionsEquivalent(unary1.Operand, unary2.Operand, out failureReason))
            {
                failureReason = $"Unary operands are different: {failureReason}";
                return false;
            }
            return true;
        }

        private static bool AreBinaryExpressionsEquivalent(BinaryExpression binary1, BinaryExpression binary2, out string failureReason)
        {
            failureReason = string.Empty;
            if (binary1.NodeType != binary2.NodeType)
            {
                failureReason = $"Binary node types are different: {binary1.NodeType} vs {binary2.NodeType}.";
                return false;
            }
            if (binary1.Method != binary2.Method)
            {
                failureReason = $"Binary methods are different: {binary1.Method} vs {binary2.Method}.";
                return false; // e.g., operator overloading
            }

            if (!AreExpressionsEquivalent(binary1.Left, binary2.Left, out failureReason))
            {
                failureReason = $"Binary left expressions are different: {failureReason}";
                return false;
            }
            if (!AreExpressionsEquivalent(binary1.Right, binary2.Right, out failureReason))
            {
                failureReason = $"Binary right expressions are different: {failureReason}";
                return false;
            }
            return true;
        }

        private static bool AreConditionalExpressionsEquivalent(ConditionalExpression cond1, ConditionalExpression cond2, out string failureReason)
        {
            failureReason = string.Empty;
            if (!AreExpressionsEquivalent(cond1.Test, cond2.Test, out failureReason))
            {
                failureReason = $"Conditional test expressions are different: {failureReason}";
                return false;
            }
            if (!AreExpressionsEquivalent(cond1.IfTrue, cond2.IfTrue, out failureReason))
            {
                failureReason = $"Conditional if-true expressions are different: {failureReason}";
                return false;
            }
            if (!AreExpressionsEquivalent(cond1.IfFalse, cond2.IfFalse, out failureReason))
            {
                failureReason = $"Conditional if-false expressions are different: {failureReason}";
                return false;
            }
            return true;
        }

        private static bool AreInvocationExpressionsEquivalent(InvocationExpression invoke1, InvocationExpression invoke2, out string failureReason)
        {
            failureReason = string.Empty;
            if (!AreExpressionsEquivalent(invoke1.Expression, invoke2.Expression, out failureReason))
            {
                failureReason = $"Invocation expressions are different: {failureReason}";
                return false;
            }

            if (invoke1.Arguments.Count != invoke2.Arguments.Count)
            {
                failureReason = "Invocation argument counts are different.";
                return false;
            }

            for (int i = 0; i < invoke1.Arguments.Count; i++)
            {
                if (!AreExpressionsEquivalent(invoke1.Arguments[i], invoke2.Arguments[i], out failureReason))
                {
                    failureReason = $"Invocation arguments at index {i} are different: {failureReason}";
                    return false;
                }
            }
            return true;
        }
    }
    #endregion
}
