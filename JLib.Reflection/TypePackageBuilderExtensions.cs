using System.Reflection;

namespace JLib.Reflection;

/// <summary>
/// 
/// </summary>
// these methods have been extracted to de-bloat on the builder class
public static class TypePackageBuilderExtensions
{
    public static TypePackageBuilder AddNestedTypes(this TypePackageBuilder builder, params Type[] containerTypes)
        => builder.Add(containerTypes.SelectMany(t => t.GetNestedTypes()).ToArray());
    public static TypePackageBuilder AddNestedTypes<TContainerType>(this TypePackageBuilder builder)
        => builder.Add(typeof(TContainerType).GetNestedTypes());
    public static TypePackageBuilder Add<TContainerType>(this TypePackageBuilder builder)
        => builder.Add(typeof(TContainerType));

    /// <summary>
    /// adds the <see cref="Assembly.GetEntryAssembly"/> which refers to the launching program.
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public static TypePackageBuilder AddEntryAssembly(this TypePackageBuilder builder)
        => builder.Add(Assembly.GetEntryAssembly());

}