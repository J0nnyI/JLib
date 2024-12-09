using FluentAssertions;

using Xunit;

namespace JLib.Helper.Tests;

public class TypeFullnameTests
{
    public class SubClassA<T>
    {
        public class SubClassB<T2>
        {

        }
        public class SubClassC<T2>
        {

        }
    }

    public class OtherClass
    {
        public class OtherSubClass { }
    }
    public class OtherGenericClass<T> { }
    [Fact]
    public void DoubleNestedGeneric()
    {
        var typeAB = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassB<OtherGenericClass<OtherClass.OtherSubClass>>
                    );
        var typeAC = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassC<OtherGenericClass<OtherClass.OtherSubClass>>
                    );

        typeAB.FullName().Should().NotBe(typeAC.FullName());

        //SubClassB needs to be present in the full name, otherwise the full name would be the same for both types
        typeAB.FullName().Should().Be(
            "TypeFullnameTests.SubClassA<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass>>"
            + ".SubClassB<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass.OtherSubClass>>");

        typeAC.FullName().Should().Be(
            "TypeFullnameTests.SubClassA<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass>>"
            + ".SubClassC<TypeFullnameTests.OtherGenericClass<TypeFullnameTests.OtherClass.OtherSubClass>>");
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
            .SubClassB<
                OtherGenericClass<SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassC<OtherGenericClass<OtherClass.OtherSubClass>>
                >
            >
        );
        typeAbc
            .FullName(true).Should().Be(
                (@"
SubClassA<OtherGenericClass<OtherClass>>
    .SubClassB<
        OtherGenericClass<SubClassA<OtherGenericClass<OtherClass>>
            .SubClassC<OtherGenericClass<OtherSubClass>>
        >
    >")
                .Replace("SubClassA", "JLib.Helper.Tests.TypeFullnameTests.SubClassA")
                .Replace("OtherGenericClass", "JLib.Helper.Tests.TypeFullnameTests.OtherGenericClass")
                .Replace("OtherClass", "JLib.Helper.Tests.TypeFullnameTests.OtherClass")
                .Replace("OtherSubClass", "JLib.Helper.Tests.TypeFullnameTests.OtherClass.OtherSubClass")
                .Replace(@"
", "")
                .Replace(" ", "")
            );
    }

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
        typeof(SubClassA<OtherRootClass.OtherClass>.SubClassB<OtherClass>)
            .FullName().Should().Be("TypeFullnameTests.SubClassA<OtherRootClass.OtherClass>.SubClassB<TypeFullnameTests.OtherClass>");
    }
}

public class OtherRootClass
{
    public class OtherClass
    {
        public class OtherSubClass { }
    }
}