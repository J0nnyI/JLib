using JLib.Reflection;
using JLib.Testing;

namespace JLib.EfCore.Tests;

public class ValidateTypePackage : ValidateTypePackageTestsBase
{
    protected override ITypePackage TypePackage => JLibEfCoreTp.Instance;
}