using System.Reflection;

using FluentAssertions;
using JLib.Reflection.Tests.DemoAssembly2;

using static JLib.Reflection.Tests.DemoAssemblyContent;
using Xunit;

namespace JLib.Reflection.Tests;

public class TypePackageBuilderTests
{
    private void RunTest(
        IReadOnlyCollection<Assembly> assembliesIn,
        IReadOnlyCollection<Type> typesIn,
        IReadOnlyCollection<Type> expectedTypes,
        Action<TypePackageBuilder>? otherHandlers = null,
        Action<ITypePackage>? validate = null
        )
    {
        var builder = new TypePackageBuilder()
            .Add(assembliesIn.ToArray())
            .Add(typesIn.ToArray());
        otherHandlers?.Invoke(builder);
        var package = builder
            .Build();

        package.GetContent().Should().Contain(expectedTypes);
        validate?.Invoke(package);
    }

    [Fact]
    public void NoPeerDependencies()
        => RunTest([Assembly2], [], Assembly2Types);
    [Fact]
    public void PeerDependencies()
        => RunTest([Assembly1], [], Assembly1Recursive);
    [Fact]
    public void PeerDependencies2()
        => RunTest([AssemblyA], [], AssemblyARecursive);
    [Fact]
    public void MultipleAssemblies()
        => RunTest([Assembly1, Assembly2, AssemblyA], [], AllAssemblyTypes);
    [Fact]
    public void ExplicitType()
        => RunTest([], DemoTypes.Types, DemoTypes.Types);

    [Fact]
    public void Nested()
        => RunTest([], [], DemoTypes.NestedTypes,
            b => b.AddNestedTypes<DemoTypes.NestingDemoClass>());

    [Fact]
    public void AssemblyBlacklist()
        => RunTest([Assembly1, AssemblyA], [],
            Assembly1Recursive.Concat(AssemblyARecursive).Except(Assembly1ATypes).Except(Assembly1A1Types).ToHashSet(),
            b => b.AddToBlacklist(Assembly1A),
            tp => tp.GetContent().Should().NotContain(Assembly1ATypes).And.NotContain(Assembly1A1Types)
        );

    [Fact]
    public void AssemblyNameBlacklist()
        => RunTest([Assembly1, AssemblyA], [],
            Assembly1Recursive.Concat(AssemblyARecursive).Except(Assembly1ATypes).Except(Assembly1A1Types).ToHashSet(),
            b => b.AddToBlacklist(Assembly1A.GetName()),
            tp => tp.GetContent().Should().NotContain(Assembly1ATypes).And.NotContain(Assembly1A1Types)
                );

    [Fact]
    public void TypeBlacklistOnAssembly()
        => RunTest([Assembly2], [],
                Assembly2Types.Except([typeof(TestAssembly2DemoClassA)]).ToArray(),
                b => b.AddToBlacklist(typeof(TestAssembly2DemoClassA)),
                tp => tp.GetContent().Should().NotContain(typeof(TestAssembly2DemoClassA)));

    [Fact]
    public void TypeBlacklistOnExplicitTypes()
        => RunTest([], DemoTypes.Types,
            DemoTypes.Types.Except([typeof(DemoTypes.DemoClassA)]).ToArray(),
            b => b.AddToBlacklist(typeof(DemoTypes.DemoClassA)),
            tp => tp.GetContent().Should().NotContain(typeof(TestAssembly2DemoClassA)));

    [Fact]
    public void TypeFilter()
        => RunTest([], DemoTypes.Types,
            DemoTypes.Types.Except([typeof(DemoTypes.DemoClassA)]).ToArray(),
            b => b.AddTypeFilter(t => t != typeof(DemoTypes.DemoClassA)),
            tp => tp.GetContent().Should().NotContain(typeof(TestAssembly2DemoClassA)));

    [Fact]
    public void MultipleNested()
        => RunTest([], [],
            DemoTypes.NestedTypes2.Concat(DemoTypes.NestedTypes2).ToArray(),
            b => b.AddNestedTypes<DemoTypes.NestingDemoClass>().AddNestedTypes<DemoTypes.NestingDemoClass2>());

    [Fact]
    public void ByFileSystem()
        => RunTest([], [],
            AllAssemblyTypes,
            b => b.AddFromPath(null, ["JLib"]));
}
