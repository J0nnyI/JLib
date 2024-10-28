using JLib.Reflection;
using JLib.Testing;

namespace JLib.Cqrs.Tests;

public class ValidateTypePackage : ValidateTypePackageTestsBase
{
    protected override ITypePackage TypePackage => JLibCqrsTp.Instance;
}