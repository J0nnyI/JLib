using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using JLib.ValueTypes;
using JLib.Helper;

namespace JLib.Tools.Benchmarks;

[InProcess]
public partial class PerfTest
{
    private static readonly Random Random = new Random();

    public record FiveCharacterStringVt(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(5);
    }

    public static FiveCharacterStringVt FiveCharacterStringCreate(Func<string> rand) => ValueTypes.ValueType.Create<FiveCharacterStringVt, string>(rand());

    public static string GetRandom5LetterString()
        => Random.Next(10000, 99999).ToString();

    public class CheckPerf
    {
        [Params(5, 0)]
        public int Value { get; set; } = 5;

        [Benchmark]
        public bool[] EqualityCheck()
            => Enumerable.Range(0, 1000).Select(x => Value == x).ToArray();

        [Benchmark]
        public bool[] InEqualityCheck()
            => Enumerable.Range(0, 1000).Select(x => Value > x).ToArray();
    }

    public class Config : ManualConfig
    {
        public Config()
        {
            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }

    [Config(typeof(Config))]
    [InProcess]
    public class TypeFullNameBenchmark
    {
        public class GenericA<TA>
        {
            public class GenericAa<TAa1, TAa2>
            {

            }
        }
        public class GenericB<TA>
        {
        }
        public class A { }
        public class B { }
        public class C { }
        [Benchmark]
        public string TypeFullName()
        {
            return typeof(GenericA<GenericB<A>>.GenericAa<B, C>).FullName(true);
        }
    }
    [Config(typeof(Config))]
    [InProcess]
    public class StringPerformanceBenchmarks
    {

        [Benchmark]
        public string FiveCharacterString() 
            => FiveCharacterStringCreate(GetRandom5LetterString).Value;

        [Benchmark]
        public string ConcatenationOperator() 
            => GetRandom5LetterString() + GetRandom5LetterString() + GetRandom5LetterString() 
               + GetRandom5LetterString() + GetRandom5LetterString();

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Initialize any setup code here if needed
        }

        [Benchmark]
        public string StringInterpolation() 
            => $"{GetRandom5LetterString()}{GetRandom5LetterString()}{GetRandom5LetterString()}{GetRandom5LetterString()}{GetRandom5LetterString()}";
    }
}