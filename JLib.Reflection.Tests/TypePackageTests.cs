using System.Reflection;
using System.Runtime.CompilerServices;

using FluentAssertions;

using JLib.Helper;

using Snapshooter.Xunit;

using Xunit;
using Xunit.Abstractions;

using static JLib.Reflection.Tests.DemoTypes;
#pragma warning disable CS0618 // Type or member is obsolete

namespace JLib.Reflection.Tests;

public class TypePackageTests(ITestOutputHelper toh)
{
    #region Assembly
    [Fact]
    public void SingleAssemblyWithNameTemplate()
    => RunTest(
        TypePackage.Get(DemoAssemblyContent.Assembly1, "Testing Assembly {0} {1}"), DemoAssemblyContent.Assembly1Types
);
    [Fact]
    public void SingleAssembly()
    => RunTest(
        TypePackage.Get(DemoAssemblyContent.Assembly1), DemoAssemblyContent.Assembly1Types
);
    [Fact]
    public void MultiAssemblyParams()
    => RunTest(
        TypePackage.Get(DemoAssemblyContent.Assembly1, DemoAssemblyContent.Assembly2), DemoAssemblyContent.Assembly1Types.Concat(DemoAssemblyContent.Assembly2Types)
);
    [Fact]
    public void MultiAssemblyCollection()
    => RunTest(
        TypePackage.Get(new[]
            {
                DemoAssemblyContent.Assembly1, DemoAssemblyContent.Assembly2
            }.CastTo<IReadOnlyCollection<Assembly>>()), DemoAssemblyContent.Assembly1Types.Concat(DemoAssemblyContent.Assembly2Types)
);
    #endregion
    #region explicit type
    [Fact]
    public void SingleTypeAssembly()
    => RunTest(
        TypePackage.Get(typeof(DemoClassA)),
        [typeof(DemoClassA)]
    );
    [Fact]
    public void MultiTypeAssemblyParams()
    => RunTest(
        TypePackage.Get(DemoTypes.Types.ToArray()),
        DemoTypes.Types
);
    [Fact]
    public void MultiTypeAssemblyCollection()
    => RunTest(
        TypePackage.Get(DemoTypes.Types),
        DemoTypes.Types
);
    #endregion
    #region nested type

    [Fact]
    public void NestedSingleArg()
        => RunTest(
            TypePackage.GetNested(typeof(NestingDemoClass)),
            NestedTypes
        );
    [Fact]
    public void NestedSingleTypeArg()
        => RunTest(
            TypePackage.GetNested<NestingDemoClass>(),
            NestedTypes
    );

    [Fact]
    public void NestedMultiParams()
        => RunTest(
            TypePackage.GetNested(typeof(NestingDemoClass), typeof(NestingDemoClass2)),
            NestedTypes.Concat(NestedTypes2)
        );
    #endregion
    #region Assembly and Types combined

    [Fact]
    public void CombinedAssembliesOnly()
        => RunTest(
            TypePackage.Get([DemoAssemblyContent.Assembly1, DemoAssemblyContent.Assembly2], []), DemoAssemblyContent.Assembly1Types.Concat(DemoAssemblyContent.Assembly2Types)
        );
    [Fact]
    public void CombinedTypesOnly()
        => RunTest(
                TypePackage.Get([], DemoTypes.Types),
                DemoTypes.Types
            );

    [Fact]
    public void CombinedSource()
        => RunTest(
            TypePackage.Get([DemoAssemblyContent.Assembly1, DemoAssemblyContent.Assembly2], DemoTypes.Types), DemoAssemblyContent.Assembly1Types.Concat(DemoAssemblyContent.Assembly2Types).Concat(DemoTypes.Types)
        );
    #endregion
    [Fact]
    public void Merged()
        => RunTest(
            TypePackage.Get(
                TypePackage.Get(typeof(DemoClassA)),
                TypePackage.Get(typeof(DemoClassB), typeof(DemoClassC))
            ),
            DemoTypes.Types);
    [Fact]
    public void ByFileSystem()
    {
        RunTest(
            TypePackage.Get(null, ["JLib.Reflection.Tests.Demo"]),
            DemoAssemblyContent.AllAssemblyTypes);
    }

    private void RunTest(
         ITypePackage package, IEnumerable<Type> expectedTypes, [CallerMemberName] string name = "")
    {
        toh.WriteLine("Type Package Content:");
        package = package.ApplyFilter(x => x.Name.Contains("Demo"));
        toh.WriteLine(package.ToJson());
        // .net 7 adds some attributes which are not included in any other .net version,
        // which means we have to remove them from the result to match all other versions
        package
            .GetContent()
            .Should()
            .OnlyContain(t => expectedTypes.Contains(t)
#if NET7_0
                || new[] { "EmbeddedAttribute", "RefSafetyRulesAttribute" }.Contains(t.Name)
#endif
            );
        expectedTypes.Should().OnlyContain(t => package.GetContent().Contains(t));
        package.ToJson()
#if NET7_0
            .RemoveSubstringsWhere((prev, cur, next)
                    =>
                    cur.Contains("Microsoft.CodeAnalysis")
                    || cur.Contains("EmbeddedAttribute")
                    || (
                        cur.Contains("],")
                        && prev?.Contains("EmbeddedAttribute") is true
                    )
                    || cur.Contains("System.Runtime.CompilerServices")
                    || cur.Contains("RefSafetyRulesAttribute")
                    || (
                        cur.Contains("],")
                        && prev?.Contains("RefSafetyRulesAttribute") is true
                    ),
                    Environment.NewLine
                )
#endif
            .MatchSnapshot(
                $"{nameof(TypePackageTests)}.{name}"

            );
    }
}
