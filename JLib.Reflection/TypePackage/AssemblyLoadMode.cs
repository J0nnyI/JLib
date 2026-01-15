using System.Reflection;

namespace JLib.Reflection;

/// <summary>
/// Controls, how the <see cref="TypePackageBuilder"/> loads the <see cref="Assembly"/>s.<br/>
/// </summary>
public enum AssemblyLoadMode
{
    /// <summary>
    /// Only the given assembly will be added, peer dependencies will not be loaded
    /// </summary>
    TopLevelOnly,
    /// <summary>
    /// The given assembly and all its peer dependencies will be loaded
    /// </summary>
    Recursive
}