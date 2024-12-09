using FluentAssertions;

using Xunit;

namespace JLib.Helper.Tests;

public class TypeFullnameTests
{
    public class SubClassA<T>
    {
        public class SubClassAA<T2>
        {

        }

        public class SubClassAB<T2>
        {

        }
    }

    public class SubClassB<TB1, TB2>
    {

        public class SubClassBA<TBA1, TBA2>
        {

            public class SubClassBAA<TBA1, TBA2>
            {

            }
        }
        public class SubClassBB<TBB1, TBB2>
        {

        }
    }
    public class X { }
    public class A : X { }
    public class B : X { }
    public class C : X { }
    public class D : X { }
    public class E : X { }
    public class F : X { }
    public class OtherClass
    {
        public class OtherSubClass { }
    }
    public class OtherGenericClass<T> { }
    [Fact]
    public void DoubleNestedGeneric()
    {
        var typeAB = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassAA<OtherGenericClass<OtherClass.OtherSubClass>>
                    );
        var typeAC = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassAB<OtherGenericClass<OtherClass.OtherSubClass>>
                    );

        typeAB.FullName().Should().NotBe(typeAC.FullName());

        //SubClassB needs to be present in the full name, otherwise the full name would be the same for both types
        typeAB.FullName().Should().Be(
            "TypeFullnameTests.SubClassA<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass>>"
            + ".SubClassAA<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass.OtherSubClass>>");

        typeAC.FullName().Should().Be(
            "TypeFullnameTests.SubClassA<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass>>"
            + ".SubClassAB<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass.OtherSubClass>>");
    }
    [Fact]
    public void NestedGeneric()
    {
        typeof(SubClassA<int>)
            .FullName().Should().Be("TypeFullnameTests.SubClassA<Int32>");
    }
    [Fact]
    public void Nested()
    {
        typeof(OtherClass.OtherSubClass)
            .FullName().Should().Be("TypeFullnameTests.OtherClass.OtherSubClass");
    }
    [Fact]
    public void TripleNestedGeneric()
    {
        var typeAbc = typeof(
            SubClassA<OtherGenericClass<OtherClass>>
            .SubClassAA<
                OtherGenericClass<SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassAB<OtherGenericClass<OtherClass.OtherSubClass>>
                >
            >
        );
        typeAbc
            .FullName(true).Should().Be(
                (@"
SubClassA<OtherGenericClass<OtherClass>>
    .SubClassAA<
        OtherGenericClass<SubClassA<OtherGenericClass<OtherClass>>
            .SubClassAB<OtherGenericClass<OtherSubClass>>
        >
    >")
                .Replace("SubClassA<", "JLib.Helper.Tests.TypeFullnameTests.SubClassA<")
                .Replace("OtherGenericClass", "JLib.Helper.Tests.TypeFullnameTests.OtherGenericClass")
                .Replace("OtherClass", "JLib.Helper.Tests.TypeFullnameTests.OtherClass")
                .Replace("OtherSubClass", "JLib.Helper.Tests.TypeFullnameTests.OtherClass.OtherSubClass")
                .Replace(@"
", "")
                .Replace(" ", "")
            );
    }
    [Fact]
    public void MultipleTypeArgumentsNestedGeneric()
    {
        var typeAbc = typeof(SubClassB<A, B>.SubClassBA<C, D>);


        typeAbc
            .FullName(false).Should().Be(
                SubClassBaName(AName, BName, CName, DName));

    }
    [Fact]
    public void MultipleTypeArgumentsNestedGenericNoNamespace()
    {
        var typeAbc = typeof(SubClassB<A, B>.SubClassBA<C, D>);

        _includeNamespace = false;

        typeAbc
            .FullName(_includeNamespace).Should().Be(
                SubClassBaName(AName, BName, CName, DName));

    }
    [Fact]
    public void MultipleTypeArgumentsNestedGenericNoNamespace2()
    {
        var typeAbc = typeof(SubClassB<A, B>.SubClassBA<C, D>.SubClassBAA<E, F>);

        _includeNamespace = false;

        typeAbc
            .FullName(_includeNamespace).Should().Be(
                SubClassBaaName(AName, BName, CName, DName, EName, FName));

    }
    [Fact]
    public void MultiGenericAsGeneric()
    {
        var typeAbc = typeof(SubClassB<A, SubClassB<B, C>>);


        typeAbc
            .FullName(_includeNamespace).Should().Be(
                SubClassBName(AName, SubClassBName(BName, CName)));
    }
    [Fact]
    public void GenericTypeDefinition()
    {
        var typeAbc = typeof(SubClassB<,>);
        typeAbc
            .FullName(_includeNamespace).Should().Be(
                SubClassBName("TB1", "TB2"));
    }
    [Fact]
    public void GenericTypeDefinition2()
    {
        var typeAbc = typeof(SubClassB<,>.SubClassBA<,>);
        typeAbc
            .FullName(_includeNamespace).Should().Be(
                SubClassBaName("TB1", "TB2", "TBA1", "TBA2"));
    }
    [Fact]
    public void GenericTypeDefinition3()
    {
        _includeNamespace = false;
        var typeAbc = typeof(SubClassB<,>.SubClassBA<,>);
        typeAbc
            .FullName(_includeNamespace).Should().Be(
                SubClassBaName("TB1", "TB2", "TBA1", "TBA2"));
    }


    private static bool _includeNamespace = true;
    static string GetName(string className, string ns = "JLib.Helper.Tests")
        => (_includeNamespace ? (ns + '.') : "") + className;
    static string AName => GetName("TypeFullnameTests.A");
    static string BName => GetName("TypeFullnameTests.B");
    static string CName => GetName("TypeFullnameTests.C");
    static string DName => GetName("TypeFullnameTests.D");
    static string EName => GetName("TypeFullnameTests.E");
    static string FName => GetName("TypeFullnameTests.F");
    static string SubClassBName(string t1, string t2)
        => GetName($"TypeFullnameTests.SubClassB<{t1}, {t2}>");

    static string SubClassBaName(string tB1, string tB2, string tBa1, string tBa2)
        => $"{SubClassBName(tB1, tB2)}.SubClassBA<{tBa1}, {tBa2}>";
    static string SubClassBaaName(string tB1, string tB2, string tBa1, string tBa2, string tBaa1, string tBaa2)
        => $"{SubClassBaName(tB1, tB2, tBa1, tBa2)}.SubClassBAA<{tBaa1}, {tBaa2}>";
    [Fact]
    public void NestedNaming()
    {
        //Issue: there are 2 classes: A.B.C and B.C in some cases, A.B.C would be serialized as B.C, leading to A.B.C.FullName == B.C.FullName
        // this test makes sure that this will not happen again

        typeof(OtherClass.OtherSubClass)
            .FullName().Should().NotBe(typeof(OtherRootClass.OtherClass.OtherSubClass).FullName());
    }
    [Fact]
    public void GenericsAreAssociatedCorrectly()
    {
        typeof(SubClassA<OtherRootClass.OtherClass>.SubClassAA<OtherClass>)
            .FullName().Should().Be("TypeFullnameTests.SubClassA<OtherRootClass.OtherClass>.SubClassAA<TypeFullnameTests.OtherClass>");
    }
}

public class OtherRootClass
{
    public class OtherClass
    {
        public class OtherSubClass { }
    }
}