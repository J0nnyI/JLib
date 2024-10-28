using JLib.Reflection;
using JLib.Testing;

namespace JLib.HotChocolate.Tests;

public class ValidateTypePackage : ValidateTypePackageTestsBase
{
    protected override ITypePackage TypePackage => JLibHotChocolateTp.Instance;
}