using System.Reflection;

using FluentAssertions;
using JLib.Exceptions;
using JLib.Exceptions.CommonExceptions;
using JLib.Helper;
using JLib.Reflection.Tests.DemoAssembly2;
using JLib.Reflection.Tests.DemoAssembly1A;

using static JLib.Reflection.Tests.DemoAssemblyContent;
using Xunit;

namespace JLib.Reflection.Tests;

public class TypePackageBuilderTests
{
    private void RunTest(
        IReadOnlyCollection<Assembly>? topOnlyAssembliesIn = null,
        IReadOnlyCollection<Assembly>? recursiveAssemblies = null,
        IReadOnlyCollection<Type>? types = null,
        IReadOnlyCollection<Type>? expectedTypes = null,
        Action<TypePackageBuilder>? additionalSetup = null,
        Action<ITypePackage>? additionalValidation = null,
        int maxIterationDepth = 100
        )
    {
        topOnlyAssembliesIn ??= [];
        recursiveAssemblies ??= [];
        types ??= [];
        expectedTypes ??= [];


        var exceptions = new ExceptionBuilder(nameof(RunTest));
        var builder = new TypePackageBuilder(options: new()
        {
            MaxDepth = maxIterationDepth
        })
            .Add(AssemblyLoadMode.Recursive, recursiveAssemblies.ToArray())
            .Add(AssemblyLoadMode.TopLevelOnly, topOnlyAssembliesIn.ToArray())
            .Add(types.ToArray());
        additionalSetup?.Invoke(builder);
        var package = builder
            .Build(exceptions);

        exceptions.ThrowIfNotEmpty();
        package.GetContent().Should().Contain(expectedTypes);
        additionalValidation?.Invoke(package);
    }

    [Fact]
    public void MixedDependencyTest()
        => RunTest(
            recursiveAssemblies: [Assembly1],
            topOnlyAssembliesIn: [Assembly2],
            expectedTypes: Assembly2Types
                .Concat(Assembly1Recursive)
                .ToReadOnlyCollection());
    [Fact]
    public void NoPeerDependencies()
        => RunTest(
            topOnlyAssembliesIn: [Assembly2],
            expectedTypes: Assembly2Types);
    [Fact]
    public void PeerDependencies()
        => RunTest(
            recursiveAssemblies: [Assembly1],
            expectedTypes: Assembly1Recursive
            );

    [Fact]
    public void PeerDependencies2()
        => RunTest(
            recursiveAssemblies: [AssemblyA],
            expectedTypes: AssemblyARecursive
            );

    [Fact]
    public void MultipleAssemblies()
        => RunTest(
            recursiveAssemblies: [Assembly1, Assembly2, AssemblyA],
            expectedTypes: AllAssemblyTypes
            );

    [Fact]
    public void ExplicitType()
        => RunTest(
            types: DemoTypes.Types,
            expectedTypes: DemoTypes.Types
            );

    [Fact]
    public void Nested()
        => RunTest(
            additionalSetup: b => b.AddNestedTypes<DemoTypes.NestingDemoClass>(),
            expectedTypes: DemoTypes.NestedTypes
        );

    [Fact]
    public void AssemblyBlacklist()
        => RunTest(
            recursiveAssemblies: [Assembly1, AssemblyA],
            additionalSetup: b => b.AddToBlacklist(Assembly1A),
            expectedTypes: Assembly1Recursive
                .Concat(AssemblyARecursive)
                .Except(Assembly1ATypes)
                .Except(Assembly1A1Types)
                .ToHashSet(),
            additionalValidation: tp => tp
                .GetContent()
                .Should()
                .NotContain(Assembly1ATypes)
                .And
                .NotContain(Assembly1A1Types)
            );

    [Fact]
    public void MaxDepthTest()
        => ((Action)(() => RunTest(
                recursiveAssemblies: [Assembly1],
                maxIterationDepth: 1
            )))
            .Should()
            .Throw<AggregateException>()
            .Where(ex => ex
                .FlattenAll()
                .OfType<MaxIterationDepthReachedException>()
                .Count() == 1);

    [Fact]
    public void AssemblyNameBlacklist()
        => RunTest(
            recursiveAssemblies: [Assembly1, AssemblyA],
            additionalSetup: b => b.AddToBlacklist(Assembly1A.GetName()),
            expectedTypes: Assembly1Recursive.Concat(AssemblyARecursive).Except(Assembly1ATypes).Except(Assembly1A1Types).ToHashSet(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(Assembly1ATypes).And.NotContain(Assembly1A1Types)
                );

    [Fact]
    public void TypeBlacklistOnAssembly()
        => RunTest(
            recursiveAssemblies: [Assembly2],
            additionalSetup: b => b.AddToBlacklist(typeof(TestAssembly2DemoClassA)),
            expectedTypes: Enumerable.Except(Assembly2Types, [typeof(TestAssembly2DemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssembly2DemoClassA)));

    [Fact]
    public void TypeBlacklistOnExplicitTypes()
        => RunTest(
            types: DemoTypes.Types,
            additionalSetup: b => b.AddToBlacklist(typeof(DemoTypes.DemoClassA)),
            expectedTypes: Enumerable.Except(DemoTypes.Types, [typeof(DemoTypes.DemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssembly2DemoClassA))
            );

    [Fact]
    public void TypeFilter()
        => RunTest([],
            types: DemoTypes.Types,
            additionalSetup: b => b.AddTypeFilter(t => t != typeof(DemoTypes.DemoClassA)),
            expectedTypes: Enumerable.Except(DemoTypes.Types, [typeof(DemoTypes.DemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssembly2DemoClassA))
            );

    [Fact]
    public void MultipleNested()
        => RunTest(
            additionalSetup: b => b.AddNestedTypes<DemoTypes.NestingDemoClass>().AddNestedTypes<DemoTypes.NestingDemoClass2>(),
            expectedTypes: DemoTypes.NestedTypes2.Concat(DemoTypes.NestedTypes2).ToArray()
            );

    [Fact]
    public void ByFileSystem()
        => RunTest(
            additionalSetup: b => b.AddFromPath(null, ["JLib"]),
            expectedTypes: AllAssemblyTypes
            );

    [Fact]
    public void AssemblyFilter()
    => RunTest(
        recursiveAssemblies: [Assembly1],
        additionalSetup: b => b.AddAssemblyFilter(assembly => assembly != Assembly1A),
        expectedTypes: Assembly1Recursive.Except(Assembly1ATypes).ToArray(),
        additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssembly1ADemoClassA))
        );
}
