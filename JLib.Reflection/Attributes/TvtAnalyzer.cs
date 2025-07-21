using JLib.Helper;

namespace JLib.Reflection;
public static class TvtAnalyzer
{
    public class TypeValueTypeAnalyzerResult(TvtFactoryAttribute attribute, Type appliedType)
    {
        public TvtFactoryAttribute Attribute { get; } = attribute;
        public Type AppliedType { get; } = appliedType;
        public bool Result { get; } = attribute.Filter(appliedType);
        public override string ToString() => Attribute.GetType().FullName() + ": " + Result;
    }
    public static IReadOnlyCollection<TypeValueTypeAnalyzerResult> CheckTypeStatus<TTypeValueType, TType>()
        where TTypeValueType : TypeValueType
    {
        return typeof(TTypeValueType).GetCustomAttributes<TvtFactoryAttribute>()
            .Select(a => new TypeValueTypeAnalyzerResult(a, typeof(TType)))
            .ToArray();
    }
}
