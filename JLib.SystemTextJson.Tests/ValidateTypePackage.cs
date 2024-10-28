using JLib.Reflection;
using JLib.Testing;

namespace JLib.SystemTextJson.Tests;

public class ValidateTypePackage : ValidateTypePackageTestsBase
{
    protected override ITypePackage TypePackage => JLibSystemTextJsonTp.Instance;
}