using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.TypeSystem.Abstractions;

public class TypeSystemValues
{
    public record MemberName(string Value) : StringValueType(Value)
    {
        [Validation]
        public static void Validate(IValidationContext<string> must)
            => must.BeAlphanumeric();
        public static implicit operator MemberName(string value) => new(value);
    }
    public record Namespace(string Value) : StringValueType(Value), IUsable
    {
        [Validation]
        public static void Validate(IValidationContext<string> must)
            => must.SatisfyCondition(x =>
            x.IsLetterOrDigit()
            || x == '.', "be alphanumeric or dot");

        public void Validate(ExceptionBuilder errors) {}

        public void Write(ISourceCodeWriter writer, ExceptionBuilder exceptions)
        {
            writer.Write("namespace ").Write(Value).EndStatement();
        }
        public static implicit operator Namespace(string value) => new(value);
    }
    public abstract record PropertyCode : StringValueType
    {
        private protected PropertyCode(string Value) : base(Value)
        {
        }
    }

    public record PropertyGetterCode(string Value) : PropertyCode(Value)
    {
        public static implicit operator PropertyGetterCode(string value) => new(value);
    }

    public record PropertySetterCode(string Value) : PropertyCode(Value)
    {
        public static implicit operator PropertySetterCode(string value) => new(value);
    }
}
