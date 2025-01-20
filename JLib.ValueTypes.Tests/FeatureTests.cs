using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JLib.ValueTypes.Tests;
public class FeatureTests
{
    private readonly Random Random = new Random();
    public record FiveCharacterStringVt(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(5);
    }
    public string GetRandom5LetterString()
        => Random.Next(10000, 99999).ToString();
    public FiveCharacterStringVt FiveCharacterStringCreate(Func<string> rand)
        => ValueType.Create<FiveCharacterStringVt, string>(rand());
    [Fact]
    public void FiveCharacterString()
        => FiveCharacterStringCreate(GetRandom5LetterString);
}
