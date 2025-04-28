using System.Reflection;

namespace JLib.Reflection;

/// <summary>
/// 
/// </summary>
// these methods have been extracted to de-bloat on the builder class
public static class TypePackageBuilderExtensions
{
    /// <summary>
    /// adds all <see cref="Type.GetNestedTypes()"/> of the given <paramref name="containerTypes"/> but not the <paramref name="containerTypes"/> themselves to the <see cref="ITypePackage"/>.
    /// </summary>
    /// <param name="builder">the <see cref="TypePackageBuilder"/> to be used</param>
    /// <param name="containerTypes">the <see cref="Type"/>s, the <see cref="Type.GetNestedTypes()"/> of which will be added to the <see cref="ITypePackage"/></param>
    /// <returns>the <paramref name="builder"/></returns>
    public static TypePackageBuilder AddNestedTypes(this TypePackageBuilder builder, params Type[] containerTypes)
        => builder.Add(containerTypes.SelectMany(t => t.GetNestedTypes()).ToArray());

    /// <summary>
    /// adds all <see cref="Type.GetNestedTypes()"/> of the given <typeparamref name="TContainerType"/> but not the <typeparamref name="TContainerType"/> themselves to the <see cref="ITypePackage"/>.
    /// </summary>
    /// <param name="builder">the <see cref="TypePackageBuilder"/> to be used</param>
    /// <typeparam name="TContainerType">the <see cref="Type"/>s, the <see cref="Type.GetNestedTypes()"/> of which will be added to the <see cref="ITypePackage"/></typeparam> <returns>the <paramref name="builder"/></returns>
    public static TypePackageBuilder AddNestedTypes<TContainerType>(this TypePackageBuilder builder)
        => builder.Add(typeof(TContainerType).GetNestedTypes());


    public static TypePackageBuilder Add<TType>(this TypePackageBuilder builder)
        => builder.Add(typeof(TType));

    /// <summary>
    /// Adds the given <paramref name="assemblies"/> to the <see cref="ITypePackage"/>.<br/>
    /// This will also add all recursive peer dependencies of this <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public static TypePackageBuilder Add(this TypePackageBuilder builder, params Assembly?[] assemblies)
        => builder.Add(AssemblyLoadMode.Recursive, assemblies);

    /// <summary>
    /// Adds the given <paramref name="assemblyNames"/> to the <see cref="ITypePackage"/>.<br/>
    /// This will also add all recursive peer dependencies of this <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public static TypePackageBuilder Add(this TypePackageBuilder builder, params AssemblyName?[] assemblyNames)
        => builder.Add(AssemblyLoadMode.Recursive, assemblyNames);

    public static TypePackageBuilder AddFromPath(this TypePackageBuilder builder, string? directory,
        IReadOnlyCollection<string> includedPrefixes, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        directory ??= AppDomain.CurrentDomain.BaseDirectory;
        var assemblyNames = Directory.EnumerateFiles(directory, "*.dll", searchOption)
            .Where(file =>
        {
            var filename = Path.GetFileName(file);
            return includedPrefixes.Any(p => filename.StartsWith(p));
        }).Select(AssemblyName.GetAssemblyName).ToArray();

        return builder.Add(assemblyNames);
    }

    // this does not work properly, since only loaded assemblies are considered for references and would lead to an incomplete package.
    // loading assemblies by filePath seems to be much more reliable.
    // /// <summary>
    // /// adds the <see cref="Assembly.GetEntryAssembly"/> which refers to the launching program.
    // /// </summary>
    // /// <returns><see langword="this"/> instance</returns>
    //public static TypePackageBuilder AddEntryAssembly(this TypePackageBuilder builder)
    //    => builder.Add(Assembly.GetEntryAssembly());

}