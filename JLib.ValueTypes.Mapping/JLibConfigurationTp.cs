
using JLib.Reflection;

namespace JLib.ValueTypes.Mapping;
/// <summary>
/// Contains all <see cref="Type"/>s required by this assembly
/// </summary>
[TypePackageProvider]
public static class JLibValueTypesMappingTp
{
    /// <summary>
    /// <inheritdoc cref="JLibValueTypesMappingTp"/>
    /// </summary>
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibValueTypesMappingTp).Assembly);
}
