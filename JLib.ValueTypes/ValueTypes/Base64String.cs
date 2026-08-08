using System.Text.RegularExpressions;

namespace JLib.ValueTypes;

/// <summary>
/// represents a base64 Value
/// </summary>
public partial record Base64String(string Value) : ValueType<string>(Value)
{
    private const string RegexExpressionConst = "^[-A-Za-z0-9+/]*={0,3}$";
#if NET6_0
    private static readonly Lazy<Regex> LazyRegex=new(()=>new(RegexExpressionConst,RegexOptions.Compiled|RegexOptions.IgnoreCase));
    private static Regex Regex() => LazyRegex.Value;
    #else
    [GeneratedRegex(RegexExpressionConst, RegexOptions.IgnoreCase)]
    private static partial Regex Regex();
#endif
    [Validation]
    private static void Validate(ValidationContext<string> must)
    {
        must.MatchRegex(Regex(), "be a valid Base64 string");
    }
}