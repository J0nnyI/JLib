using JLib.Reflection;
using JLib.Testing;

namespace JLib.AutoMapper.Tests;

public class ValidateTypePackage : ValidateTypePackageTestsBase
{
    protected override ITypePackage TypePackage => JLibAutoMapperTp.Instance;
}