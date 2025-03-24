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