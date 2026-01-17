using System.Reflection;
using FluentAssertions;
using JLib.Helper;
using Xunit;

namespace JLib.Helper.Tests;

public class CustomTypeAttributeCacheTests
{
    private readonly CustomTypeAttributeCache _cache = new();

    #region Test Attributes and Classes
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class TestAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class NonInheritedTestAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }

    [Test("Base")]
    public class BaseClass { }

    [Test("Derived")]
    public class DerivedClass : BaseClass { }

    [Test("Multiple1")]
    [Test("Multiple2")]
    public class MultipleAttributeClass { }

    [NonInheritedTest("Base")]
    public class NonInheritedBaseClass { }

    public class NonInheritedDerivedClass : NonInheritedBaseClass { }

    public class NoAttributeClass { }
    #endregion

    [Fact]
    public void GetCustomAttributes_Generic_Inheritance_Disabled_ShouldNotReturnInherited()
    {
        // Arrange
        var type = typeof(NonInheritedDerivedClass);

        // Act
        var attributes = _cache.GetCustomAttributes<NonInheritedTestAttribute>(type, true);

        // Assert
        attributes.Should().BeEmpty();
    }

    [Fact]
    public void GetCustomAttributes_Generic_ShouldReturnAttributes()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var attributes = _cache.GetCustomAttributes<TestAttribute>(type);

        // Assert
        attributes.Should().ContainSingle()
            .Which.Value.Should().Be("Base");
    }

    [Fact]
    public void GetCustomAttributes_Generic_Multiple_ShouldReturnAllAttributes()
    {
        // Arrange
        var type = typeof(MultipleAttributeClass);

        // Act
        var attributes = _cache.GetCustomAttributes<TestAttribute>(type);

        // Assert
        attributes.Should().HaveCount(2);
        attributes.Select(a => a.Value).Should().BeEquivalentTo("Multiple1", "Multiple2");
    }

    [Fact]
    public void GetCustomAttributes_Generic_Inheritance_ShouldWork()
    {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var inherited = _cache.GetCustomAttributes<TestAttribute>(type, true);
        var notInherited = _cache.GetCustomAttributes<TestAttribute>(type, false);

        // Assert
        inherited.Should().HaveCount(2);
        inherited.Select(a => a.Value).Should().BeEquivalentTo("Base", "Derived");
        notInherited.Should().ContainSingle().Which.Value.Should().Be("Derived");
    }

    [Fact]
    public void GetCustomAttributes_NonGeneric_ShouldReturnAttributes()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var attributes = _cache.GetCustomAttributes(type, typeof(TestAttribute));

        // Assert
        attributes.Should().ContainSingle()
            .Which.Should().BeOfType<TestAttribute>()
            .Which.Value.Should().Be("Base");
    }

    [Fact]
    public void GetCustomAttributes_NonGeneric_InvalidType_ShouldThrow()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var act = () => _cache.GetCustomAttributes(type, typeof(string));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("String is not assignable to Attribute");
    }

    [Fact]
    public void GetCustomAttribute_Generic_Success()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var attribute = _cache.GetCustomAttribute<TestAttribute>(type);

        // Assert
        attribute.Value.Should().Be("Base");
    }

    [Fact]
    public void GetCustomAttribute_Generic_AmbiguousMatch_ShouldThrow()
    {
        // Arrange
        var type = typeof(MultipleAttributeClass);

        // Act
        var act = () => _cache.GetCustomAttribute<TestAttribute>(type);

        // Assert
        act.Should().Throw<AmbiguousMatchException>().WithMessage("*more than one*");
    }

    [Fact]
    public void GetCustomAttribute_Generic_NoAttribute_ShouldThrow()
    {
        // Arrange
        var type = typeof(NoAttributeClass);

        // Act
        var act = () => _cache.GetCustomAttribute<TestAttribute>(type);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*no attributes of type*");
    }

    [Fact]
    public void GetCustomAttribute_NonGeneric_Success()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var attribute = _cache.GetCustomAttribute(type, typeof(TestAttribute));

        // Assert
        attribute.Should().BeOfType<TestAttribute>()
            .Which.Value.Should().Be("Base");
    }

    [Fact]
    public void Clear_ShouldFlushCache()
    {
        // Arrange
        var type = typeof(BaseClass);
        _cache.GetCustomAttributes<TestAttribute>(type);

        // Act
        _cache.Clear();
        var attributes = _cache.GetCustomAttributes<TestAttribute>(type);

        // Assert
        attributes.Should().ContainSingle().Which.Value.Should().Be("Base");
    }

    [AttributeUsage(AttributeTargets.All)]
    public class OtherAttribute : Attribute { }

    [Test("Base")]
    [Other]
    public class MultipleTypesClass { }

    [Fact]
    public void GetCustomAttributes_NonGeneric_ShouldOnlyReturnRequestedType()
    {
        // Arrange
        var type = typeof(MultipleTypesClass);

        // Act
        var attributes = _cache.GetCustomAttributes(type, typeof(TestAttribute));

        // Assert
        attributes.Should().ContainSingle().Which.Should().BeOfType<TestAttribute>();
    }

    [Fact]
    public void Clear_Type_ShouldOnlyClearSpecifiedAttributeType()
    {
        // Arrange
        var type = typeof(BaseClass);
        _cache.GetCustomAttributes<TestAttribute>(type);
        _cache.GetCustomAttributes<NonInheritedTestAttribute>(typeof(NonInheritedBaseClass));

        // Act
        _cache.Clear(typeof(TestAttribute));
        
        // Assert
        _cache.GetCustomAttributes<TestAttribute>(type).Should().NotBeNull();
        _cache.GetCustomAttributes<NonInheritedTestAttribute>(typeof(NonInheritedBaseClass)).Should().NotBeNull();
    }

    [Fact]
    public void Clear_NonAttributeType_ShouldThrow()
    {
        // Act
        var act = () => _cache.Clear(typeof(string));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("String is not assignable to Attribute");
    }

    [Fact]
    public void GetCustomAttribute_NonGeneric_NonAttributeType_ShouldThrow()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var act = () => _cache.GetCustomAttribute(type, typeof(string));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("String is not assignable to Attribute");
    }

    [Fact]
    public void IsDefined_Generic_ShouldReturnTrue_WhenAttributeExists()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var result = _cache.IsDefined<TestAttribute>(type);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDefined_Generic_ShouldReturnFalse_WhenAttributeDoesNotExist()
    {
        // Arrange
        var type = typeof(NoAttributeClass);

        // Act
        var result = _cache.IsDefined<TestAttribute>(type);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDefined_NonGeneric_ShouldReturnTrue_WhenAttributeExists()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var result = _cache.IsDefined(type, typeof(TestAttribute));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDefined_NonGeneric_ShouldReturnFalse_WhenAttributeDoesNotExist()
    {
        // Arrange
        var type = typeof(NoAttributeClass);

        // Act
        var result = _cache.IsDefined(type, typeof(TestAttribute));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDefined_ShouldWorkWithInheritance()
    {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var resultInherit = _cache.IsDefined<TestAttribute>(type, true);
        var resultNoInherit = _cache.IsDefined<TestAttribute>(type, false);

        // Assert
        resultInherit.Should().BeTrue();
        resultNoInherit.Should().BeTrue();
    }

    [Fact]
    public void IsDefined_NonInherited_ShouldWorkWithInheritance()
    {
        // Arrange
        var type = typeof(NonInheritedDerivedClass);

        // Act
        var resultInherit = _cache.IsDefined<NonInheritedTestAttribute>(type, true);
        var resultNoInherit = _cache.IsDefined<NonInheritedTestAttribute>(type, false);

        // Assert
        resultInherit.Should().BeFalse();
        resultNoInherit.Should().BeFalse();
    }

    [Fact]
    public void GetCustomAttributes_After_IsDefined_False_ShouldWork()
    {
        // Arrange
        var type = typeof(NoAttributeClass);
        _cache.IsDefined<TestAttribute>(type).Should().BeFalse();

        // Act
        var attributes = _cache.GetCustomAttributes<TestAttribute>(type);

        // Assert
        attributes.Should().BeEmpty();
    }
}
