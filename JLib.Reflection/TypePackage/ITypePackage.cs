using System.Collections.Immutable;

namespace JLib.Reflection;

/// <summary>
/// Contains types to be used by the <see cref="TypeCache"/>.<br/>
/// Created by the <see cref="TypePackageBuilder"/>
/// </summary>
public interface ITypePackage
{
    /// <summary>
    /// The <see cref="Types"/> of this package and all its <see cref="Children"/>
    /// </summary>
    public ImmutableHashSet<Type> GetContent();
    /// <summary>
    /// other nested packages, this could be peer dependencies, the assemblies of a directory or any other group of packages
    /// </summary>
    public IEnumerable<ITypePackage> Children { get; }

    /// <summary>
    /// The types which are directly defined in this package.
    /// </summary>
    public IEnumerable<Type> Types { get; }

    /// <summary>
    /// a short description of the contents of the type package.<br/>
    /// </summary>
    public string DescriptionTemplate { get; }

    /// <summary>
    /// returns a type package which is a combination of the given packages
    /// </summary>
    [Obsolete]
    public ITypePackage Combine(params ITypePackage[] packages);
}