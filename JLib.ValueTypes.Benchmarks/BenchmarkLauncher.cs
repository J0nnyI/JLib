using BenchmarkDotNet.Running;

namespace JLib.ValueTypes.Benchmarks;
public static class BenchmarkLauncher
{
    public static void Main()
    {
        BenchmarkRunner.Run(typeof(BenchmarkRunner).Assembly);
    }
}
