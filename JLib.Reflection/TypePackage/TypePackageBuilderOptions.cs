using System.Reflection;

namespace JLib.Reflection;

/// <summary>
/// Options which control the <see cref="TypePackageBuilder"/>s behavior
/// </summary>
public class TypePackageBuilderOptions
{
    /// <summary>
    /// The default instance of this class
    /// </summary>
    public static TypePackageBuilderOptions Default { get; } = new();
    /// <summary>
    /// Controls, how deep the <see cref="Assembly"/> peer dependency tree is allowed to get.<br/>
    /// This setting exists to break out of an endless loop.<br/>
    /// The default value of 1000 should be more than enough to not need an override.
    /// </summary>
    public int MaxDepth { get; init; } = 1000;

}