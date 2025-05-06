using System.Linq;
using System.Reflection;

using JLib.Helper;
using JLib.Reflection.Tests.DemoAssembly1;
using JLib.Reflection.Tests.DemoAssembly1A;
using JLib.Reflection.Tests.DemoAssembly1A1;
using JLib.Reflection.Tests.DemoAssembly2;
using JLib.Reflection.Tests.DemoAssemblyA;

namespace JLib.Reflection.Tests;

public static class DemoAssemblyContent
{
    public static readonly IReadOnlyCollection<Type> Assembly1Types =
    [
        typeof(TestAssemblyDemoClassA),
        typeof(TestAssemblyDemoClassB),
        typeof(TestAssemblyDemoClassC)
    ];


    public static readonly IReadOnlyCollection<Type> Assembly1ATypes =
    [
        typeof(TestAssembly1ADemoClassA),
        typeof(TestAssembly1ADemoClassB),
        typeof(TestAssembly1ADemoClassC)
    ];

    public static readonly IReadOnlyCollection<Type> Assembly1A1Types =
    [
        typeof(TestAssembly1A1DemoClassA),
        typeof(TestAssembly1A1DemoClassB),
        typeof(TestAssembly1A1DemoClassC)
    ];

    public static readonly IReadOnlyCollection<Type> AssemblyATypes =
    [
        typeof(TestAssemblyADemoClassA),
        typeof(TestAssemblyADemoClassB),
        typeof(TestAssemblyADemoClassC)
    ];

    public static readonly IReadOnlyCollection<Type> Assembly2Types =
    [
        typeof(TestAssembly2DemoClassA),
        typeof(TestAssembly2DemoClassB),
        typeof(TestAssembly2DemoClassC)
    ];

    public static readonly Assembly Assembly1 = Assembly1Types.First().Assembly;
    public static readonly Assembly Assembly1A = Assembly1ATypes.First().Assembly;
    public static readonly Assembly Assembly1A1 = Assembly1A1Types.First().Assembly;
    public static readonly Assembly AssemblyA = AssemblyATypes.First().Assembly;


    public static readonly Assembly Assembly2 = Assembly2Types.First().Assembly;


    public static IReadOnlyCollection<Type> AllAssemblyTypes { get; } = new[]
    {
        Assembly1Types,
        Assembly1ATypes,
        Assembly1A1Types,
        AssemblyATypes,
        Assembly2Types
    }.SelectMany(x => x).ToReadOnlyCollection();

    public static readonly IReadOnlyCollection<Type> Assembly1Recursive =
        new[]
        {
            Assembly1Types,
            Assembly1ATypes,
            Assembly1A1Types,
        }.SelectMany(x => x).ToReadOnlyCollection();
    public static readonly IReadOnlyCollection<Type> Assembly1ARecursive =
        new[]
        {
            Assembly1ATypes,
            Assembly1A1Types,
        }.SelectMany(x => x).ToReadOnlyCollection();
    public static readonly IReadOnlyCollection<Type> AssemblyARecursive =
        new[]
        {
            AssemblyATypes,
            Assembly1ATypes,
            Assembly1A1Types
        }.SelectMany(x => x).ToReadOnlyCollection();


}