using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// 
/// </summary>
// these methods have been extracted to de-bloat on the builder class
public static class TypePackageBuilderExtensions
{
    /// <summary>
    /// adds all recursive <see cref="Type.GetNestedTypes()"/> of the given <paramref name="containerTypes"/> but not the <paramref name="containerTypes"/> themselves to the <see cref="ITypePackage"/>.
    /// </summary>
    /// <param name="builder">the <see cref="TypePackageBuilder"/> to be used</param>
    /// <param name="containerTypes">the <see cref="Type"/>s, the <see cref="Type.GetNestedTypes()"/> of which will be added to the <see cref="ITypePackage"/></param>
    /// <returns>the <paramref name="builder"/></returns>
    public static TypePackageBuilder AddNestedTypes(this TypePackageBuilder builder, params Type[] containerTypes)
    {
        int maxIterations = 100;// this number is arbitrary and should be high enough to cover most use cases
        var types = containerTypes.SelectMany(t => t.GetNestedTypes()).ToArray();
         builder.Add(types);
        for(int i=0;i<maxIterations;i++) // basically while(types.Length > 0) but with an emergency break
        {
            types = types.SelectMany(t => t.GetNestedTypes()).ToArray();
            if (types.None())
                return builder;
            builder.Add(types);
        }

        throw new InvalidSetupException(
            $"could not add nested types to package. the type is either recursive or has too many ( > {maxIterations}) levels of nesting."
        );
    }

    /// <summary>
    /// adds all <see cref="Type.GetNestedTypes()"/> of the given <typeparamref name="TContainerType"/> but not the <typeparamref name="TContainerType"/> themselves to the <see cref="ITypePackage"/>.
    /// </summary>
    /// <param name="builder">the <see cref="TypePackageBuilder"/> to be used</param>
    /// <typeparam name="TContainerType">the <see cref="Type"/>s, the <see cref="Type.GetNestedTypes()"/> of which will be added to the <see cref="ITypePackage"/></typeparam> <returns>the <paramref name="builder"/></returns>
    public static TypePackageBuilder AddNestedTypes<TContainerType>(this TypePackageBuilder builder)
        => builder.AddNestedTypes(typeof(TContainerType));


    /// <summary>
    /// Adds <typeparamref name="TType"/> to the <see cref="ITypePackage"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
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

    /// <summary>
    /// Adds all *.dll's which <see cref="string.StartsWith(string)"/> <paramref name="includedPrefixes"/> located inside the given <paramref name="directory"/> with the <paramref name="searchOption"/> to the <see cref="ITypePackage"/>.
    /// </summary>
    public static TypePackageBuilder AddFromPath(this TypePackageBuilder builder, string? directory,
        IReadOnlyCollection<string> includedPrefixes, SearchOption searchOption = SearchOption.TopDirectoryOnly,
        AssemblyLoadMode loadMode = AssemblyLoadMode.TopLevelOnly)
    {
        directory ??= AppDomain.CurrentDomain.BaseDirectory;
        var assemblyNames = Directory.EnumerateFiles(directory, "*.dll", searchOption)
            .Where(file =>
        {
            var filename = Path.GetFileName(file);
            return includedPrefixes.Any(p => filename.StartsWith(p));
        }).Select(AssemblyName.GetAssemblyName).ToArray();

        return builder.Add(loadMode, assemblyNames);
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