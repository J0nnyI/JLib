using JLib.Reflection;
using JLib.Testing;

namespace JLib.Configuration;

public class ValidateTypePackage : ValidateTypePackageTestsBase
{
    protected override ITypePackage TypePackage => JLibConfigurationTp.Instance;
}