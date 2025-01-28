using System.Linq.Expressions;
using System.Text.Json;
namespace JLib.Helper;
using System;
using System.Reflection;
using Newtonsoft.Json;

public class TypeNameConverter : JsonConverter<Type>
{
    public override void WriteJson(JsonWriter writer, Type? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.Name ?? "none");
    }

    public override Type ReadJson(JsonReader reader, Type objectType, Type? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
public class MethodInfoNameConverter : JsonConverter<MethodInfo>
{
    public override void WriteJson(JsonWriter writer, MethodInfo? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.Name ?? "none");
    }

    public override MethodInfo ReadJson(JsonReader reader, Type objectType, MethodInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public static partial class ExpressionHelper
{
    /// <summary>
    /// Provides methods to compare expressions for equivalence.
    /// </summary>
    public static class ExpressionComparer
    {
        private static readonly JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 1,
            Converters = { new TypeNameConverter() , new MethodInfoNameConverter()},
            Formatting = Formatting.Indented

        };
        
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
                throw new ArgumentException("One of the expressions is null.");

            if (expr1.NodeType != expr2.NodeType)
                throw new ArgumentException($"Node types are different: {JsonConvert.SerializeObject(expr1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(expr2, _jsonSerializerOptions)}.");
            if (expr1.Type != expr2.Type)
                throw new ArgumentException($"Expression types are different: {JsonConvert.SerializeObject(expr1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(expr2, _jsonSerializerOptions)}.");
            switch (expr1.NodeType)
            {
                case ExpressionType.Constant:
                    return AreConstantExpressionsEquivalent((ConstantExpression)expr1, (ConstantExpression)expr2);
                case ExpressionType.Parameter:
                    return AreParameterExpressionsEquivalent((ParameterExpression)expr1, (ParameterExpression)expr2);
                case ExpressionType.Lambda:
                    return AreLambdaExpressionsEquivalent((LambdaExpression)expr1, (LambdaExpression)expr2);
                case ExpressionType.Call:
                    return AreMethodCallExpressionsEquivalent((MethodCallExpression)expr1, (MethodCallExpression)expr2);
                case ExpressionType.Convert:
                    return AreUnaryExpressionsEquivalent((UnaryExpression)expr1, (UnaryExpression)expr2);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return AreBinaryExpressionsEquivalent((BinaryExpression)expr1, (BinaryExpression)expr2);
                case ExpressionType.Conditional:
                    return AreConditionalExpressionsEquivalent((ConditionalExpression)expr1, (ConditionalExpression)expr2);
                case ExpressionType.Invoke:
                    return AreInvocationExpressionsEquivalent((InvocationExpression)expr1, (InvocationExpression)expr2);
                case ExpressionType.New:
                    return AreNewExpressionsEquivalent((NewExpression)expr1, (NewExpression)expr2);
                default:
                    throw new NotSupportedException($"Unsupported expression type: {expr1.NodeType}.");
            }
        }

        private static bool AreNewExpressionsEquivalent(NewExpression new1, NewExpression new2)
        {
            if (new1.Constructor != new2.Constructor)
                throw new ArgumentException($"Constructors are different: {JsonConvert.SerializeObject(new1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(new2, _jsonSerializerOptions)}.");

            if (new1.Arguments.Count != new2.Arguments.Count)
                throw new ArgumentException($"Constructor argument counts are different: {JsonConvert.SerializeObject(new1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(new2, _jsonSerializerOptions)}.");
            for (int i = 0; i < new1.Arguments.Count; i++)
            {
                if (!AreExpressionsEquivalent(new1.Arguments[i], new2.Arguments[i], out var failureReason))
                    throw new ArgumentException($"Constructor arguments at index {i} are different: {failureReason} - {JsonConvert.SerializeObject(new1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(new2, _jsonSerializerOptions)}.");
            }

            return true;
        }

        private static bool AreConstantExpressionsEquivalent(ConstantExpression c1, ConstantExpression c2)
        {
            if (!Equals(c1.Value, c2.Value))
                throw new ArgumentException($"Constant values are different: {JsonConvert.SerializeObject(c1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(c2, _jsonSerializerOptions)}.");
            return true;
        }

        private static bool AreParameterExpressionsEquivalent(ParameterExpression p1, ParameterExpression p2)
        {
            if (p1.Name != p2.Name || p1.Type != p2.Type)
                throw new ArgumentException($"Parameter expressions are different: {JsonConvert.SerializeObject(p1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(p2, _jsonSerializerOptions)}.");
            return true;
        }

        private static bool AreLambdaExpressionsEquivalent(LambdaExpression lambda1, LambdaExpression lambda2)
        {
            if (lambda1.Parameters.Count != lambda2.Parameters.Count)
                throw new ArgumentException($"Lambda parameter counts are different: {JsonConvert.SerializeObject(lambda1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(lambda2, _jsonSerializerOptions)}.");
            if (!AreExpressionsEquivalent(lambda1.Body, lambda2.Body, out var failureReason))
                throw new ArgumentException($"Lambda bodies are different: {failureReason} - {JsonConvert.SerializeObject(lambda1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(lambda2, _jsonSerializerOptions)}.");
            for (int i = 0; i < lambda1.Parameters.Count; i++)
            {
                if (!AreParameterExpressionsEquivalent(lambda1.Parameters[i], lambda2.Parameters[i]))
                    throw new ArgumentException($"Lambda parameters at index {i} are different: {JsonConvert.SerializeObject(lambda1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(lambda2, _jsonSerializerOptions)}.");
            }

            return true;
        }

        private static bool AreMethodCallExpressionsEquivalent(MethodCallExpression call1, MethodCallExpression call2)
        {
            if (call1.Method != call2.Method)
                throw new ArgumentException($"Method calls are different: {JsonConvert.SerializeObject(call1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(call2, _jsonSerializerOptions)}.");

            if (call1.Object != null && call2.Object != null)
            {
                if (!AreExpressionsEquivalent(call1.Object!, call2.Object!, out var failureReason)) // Instance method object
                    throw new ArgumentException($"Method call objects are different: {failureReason} - {JsonConvert.SerializeObject(call1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(call2, _jsonSerializerOptions)}.");
            }
            else if (call1.Object != call2.Object)
            {
                throw new ArgumentException($"Method call objects are different: {JsonConvert.SerializeObject(call1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(call2, _jsonSerializerOptions)}.");
            }


            if (call1.Arguments.Count != call2.Arguments.Count)
                throw new ArgumentException($"Method call argument counts are different: {JsonConvert.SerializeObject(call1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(call2, _jsonSerializerOptions)}.");

            for (int i = 0; i < call1.Arguments.Count; i++)
            {
                if (!AreExpressionsEquivalent(call1.Arguments[i], call2.Arguments[i], out var failureReasonSub))
                    throw new ArgumentException($"Method call arguments at index {i} are different: {failureReasonSub} - {JsonConvert.SerializeObject(call1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(call2, _jsonSerializerOptions)}.");
            }

            return true;
        }

        private static bool AreUnaryExpressionsEquivalent(UnaryExpression unary1, UnaryExpression unary2)
        {
            if (unary1.NodeType != unary2.NodeType)
                throw new ArgumentException($"Unary node types are different: {JsonConvert.SerializeObject(unary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(unary2, _jsonSerializerOptions)}.");
            if (unary1.Method != unary2.Method)
                throw new ArgumentException($"Unary methods are different: {JsonConvert.SerializeObject(unary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(unary2, _jsonSerializerOptions)}.");
            if (!AreExpressionsEquivalent(unary1.Operand, unary2.Operand, out var failureReason))
                throw new ArgumentException($"Unary operands are different: {failureReason} - {JsonConvert.SerializeObject(unary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(unary2, _jsonSerializerOptions)}");
            return true;
        }

        private static bool AreBinaryExpressionsEquivalent(BinaryExpression binary1, BinaryExpression binary2)
        {
            if (binary1.NodeType != binary2.NodeType)
                throw new ArgumentException($"Binary node types are different: {JsonConvert.SerializeObject(binary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(binary2, _jsonSerializerOptions)}.");
            if (binary1.Method != binary2.Method)
                throw new ArgumentException($"Binary methods are different: {JsonConvert.SerializeObject(binary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(binary2, _jsonSerializerOptions)}.");

            if (!AreExpressionsEquivalent(binary1.Left, binary2.Left, out var failureReason))
                throw new ArgumentException($"Binary left expressions are different: {failureReason} - {JsonConvert.SerializeObject(binary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(binary2, _jsonSerializerOptions)}");
            if (!AreExpressionsEquivalent(binary1.Right, binary2.Right, out var failureReasonRight))
                throw new ArgumentException($"Binary right expressions are different: {failureReasonRight} - {JsonConvert.SerializeObject(binary1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(binary2, _jsonSerializerOptions)}");
            return true;
        }

        private static bool AreConditionalExpressionsEquivalent(ConditionalExpression cond1, ConditionalExpression cond2)
        {
            if (!AreExpressionsEquivalent(cond1.Test, cond2.Test, out var failureReasonTest))
                throw new ArgumentException($"Conditional test expressions are different: {failureReasonTest} - {JsonConvert.SerializeObject(cond1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(cond2, _jsonSerializerOptions)}");
            if (!AreExpressionsEquivalent(cond1.IfTrue, cond2.IfTrue, out var failureReasonIfTrue))
                throw new ArgumentException($"Conditional if-true expressions are different: {failureReasonIfTrue} - {JsonConvert.SerializeObject(cond1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(cond2, _jsonSerializerOptions)}");
            if (!AreExpressionsEquivalent(cond1.IfFalse, cond2.IfFalse, out var failureReasonIfFalse))
                throw new ArgumentException($"Conditional if-false expressions are different: {failureReasonIfFalse} - {JsonConvert.SerializeObject(cond1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(cond2, _jsonSerializerOptions)}");
            return true;
        }

        private static bool AreInvocationExpressionsEquivalent(InvocationExpression invoke1, InvocationExpression invoke2)
        {
            if (!AreExpressionsEquivalent(invoke1.Expression, invoke2.Expression, out var failureReason))
                throw new ArgumentException($"Invocation expressions are different: {failureReason} - {JsonConvert.SerializeObject(invoke1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(invoke2, _jsonSerializerOptions)}");

            if (invoke1.Arguments.Count != invoke2.Arguments.Count)
                throw new ArgumentException($"Invocation argument counts are different: {JsonConvert.SerializeObject(invoke1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(invoke2, _jsonSerializerOptions)}.");

            for (int i = 0; i < invoke1.Arguments.Count; i++)
            {
                if (!AreExpressionsEquivalent(invoke1.Arguments[i], invoke2.Arguments[i], out var failureReasonSub))
                    throw new ArgumentException($"Invocation arguments at index {i} are different: {failureReasonSub} - {JsonConvert.SerializeObject(invoke1, _jsonSerializerOptions)} vs {JsonConvert.SerializeObject(invoke2, _jsonSerializerOptions)}");
            }
            return true;
        }
    }
}
