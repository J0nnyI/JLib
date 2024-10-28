using FluentAssertions;
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JLib.Testing;

/// <summary>
/// Checks, whether the type packages of a given assembly are valid
/// </summary>
public abstract class ValidateTypePackageTestsBase
{
    /// <summary>
    /// the type package to be tested
    /// </summary>
    protected abstract ITypePackage TypePackage { get; }

    /// <summary>
    /// <inheritdoc cref="ValidateTypePackageTestsBase"/>
    /// </summary>
    [Fact]
    public virtual void ValidateTypePackage()
    {
        var exceptions = new ExceptionBuilder(nameof(ValidateTypePackage));
        var cache = new TypeCache(TypePackage, exceptions, NullLoggerFactory.Instance);
        exceptions.GetException()?.ToString().Should().BeNull();
    }
}
