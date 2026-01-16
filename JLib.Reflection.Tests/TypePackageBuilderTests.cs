using System.Reflection;

using FluentAssertions;
using JLib.Exceptions;
using JLib.Exceptions.CommonExceptions;
using JLib.Helper;
using JLib.Reflection.Tests.DemoAssembly1;
using JLib.Reflection.Tests.DemoAssembly2;
using JLib.Reflection.Tests.DemoAssembly1A;

using static JLib.Reflection.Tests.DemoAssemblyContent;
using Xunit;
using Xunit.Abstractions;

namespace JLib.Reflection.Tests;

public class TypePackageBuilderTests(ITestOutputHelper toh)
{
    private void RunTest(
        IReadOnlyCollection<Assembly>? topOnlyAssemblies = null,
        IReadOnlyCollection<Assembly>? recursiveAssemblies = null,
        IReadOnlyCollection<Type>? types = null,
        IReadOnlyCollection<Type>? expectedTypes = null,
        Action<TypePackageBuilder>? additionalSetup = null,
        Action<ITypePackage>? additionalValidation = null,
        int maxIterationDepth = 100
        )
    {
        topOnlyAssemblies ??= [];
        recursiveAssemblies ??= [];
        types ??= [];
        expectedTypes ??= [];


        var exceptions = new ExceptionBuilder(nameof(RunTest));
        var builder = new TypePackageBuilder(options: new()
        {
            MaxDepth = maxIterationDepth
        })
            .Add(AssemblyLoadMode.Recursive, recursiveAssemblies.ToArray())
            .Add(AssemblyLoadMode.TopLevelOnly, topOnlyAssemblies.ToArray())
            .Add(types.ToArray());
        additionalSetup?.Invoke(builder);
        var package = builder
            .Build(exceptions);

        toh.WriteLine(package.ToJson());

        exceptions.ThrowIfNotEmpty();
        if (expectedTypes.Count > 0)
            package.GetContent().Should().Contain(expectedTypes);
        additionalValidation?.Invoke(package);
    }

    [Fact]
    public void MixedDependencyTest()
        => RunTest(
            recursiveAssemblies: [Assembly1],
            topOnlyAssemblies: [Assembly2],
            expectedTypes: Assembly2Types
                .Concat(Assembly1Recursive)
                .ToReadOnlyCollection());
    [Fact]
    public void NoPeerDependencies()
        => RunTest(
            topOnlyAssemblies: [Assembly2],
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
        => RunTest(
            topOnlyAssemblies: [Assembly1],
            additionalSetup: b => b.AddTypeFilter(t => t != typeof(TestAssemblyDemoClassA)),
            expectedTypes: Enumerable.Except(Assembly1Types, [typeof(TestAssemblyDemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssemblyDemoClassA))
            );

    [Fact]
    public void FilterIncludedType()// todo
        => RunTest(
            types: Assembly1Types,
            additionalSetup: b => b.AddTypeFilter(t => t != typeof(TestAssemblyDemoClassA)),
            expectedTypes: Enumerable.Except(Assembly1Types, [typeof(TestAssemblyDemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssemblyDemoClassA))
        );
    [Fact]
    public void FilterAssemblyType()
        => RunTest(
            topOnlyAssemblies: [Assembly1],
            additionalSetup: b => b.AddTypeFilter(t => t != typeof(TestAssemblyDemoClassA)),
            expectedTypes: Enumerable.Except(Assembly1Types, [typeof(TestAssemblyDemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssemblyDemoClassA))
        );
    [Fact]
    public void FilterAssemblyTypeNested()
        => RunTest(
            recursiveAssemblies: [Assembly1],
            additionalSetup: b => b.AddTypeFilter(t => t != typeof(TestAssembly1ADemoClassA)),
            expectedTypes: Enumerable.Except(Assembly1Recursive, [typeof(TestAssembly1ADemoClassA)]).ToArray(),
            additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssembly1ADemoClassA))
        );

    [Fact]
    public void FilterIncludedAssembly()
        => RunTest(
            recursiveAssemblies: [Assembly1],
            additionalSetup: b => b.AddAssemblyFilter(a => a.FullName != Assembly1.FullName),
            expectedTypes: [],
            additionalValidation: tp => tp.GetContent().Should().BeEmpty()
        );
    [Fact]
    public void FilterIncludedAssembly2()
        => RunTest(
            recursiveAssemblies: [Assembly1],
            additionalSetup: b => b.AddAssemblyFilter(a => a.FullName != Assembly1A.FullName),
            expectedTypes: Assembly1Types,
            additionalValidation: tp => tp.GetContent().Should().NotContain(Assembly1ATypes.Concat(Assembly1A1Types))
        );
    [Fact]
    public void ByFilepath()
        => RunTest(
            additionalSetup: b => b.AddFromPath(directory: null, includedPrefixes: ["JLib.Reflection.Tests.DemoAssembly"]),
            expectedTypes: AllAssemblyTypes
        );

    [Fact]
    public void MultipleLevelNested()
        => RunTest(
            additionalSetup: b => b.AddNestedTypes<DemoTypes.NestingDemoClass2>(),
            expectedTypes: DemoTypes.NestedTypes2.ToArray()
        );

    
    [Fact]
    public void MultipleNested()
        => RunTest(
            additionalSetup: b => b.AddNestedTypes<DemoTypes.NestingDemoClass>().AddNestedTypes<DemoTypes.NestingDemoClass2>(),
            expectedTypes: DemoTypes.NestedTypes.Concat(DemoTypes.NestedTypes2).ToArray()
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
        additionalSetup: b => b.AddAssemblyFilter(assembly => assembly.FullName != Assembly1A.GetName().FullName),
        expectedTypes: Assembly1Recursive.Except(Assembly1ATypes).Except(Assembly1A1Types).ToArray(),
        additionalValidation: tp => tp.GetContent().Should().NotContain(typeof(TestAssembly1ADemoClassA))
        );
}