using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// Extension Methods for loading Assemblies
/// </summary>
public static class AssemblyLoaderExtensions
{
    /// <summary>
    /// tries to load the given <paramref name="assembly"/>. Returns <see langword="null"/> if the load failed and adds a <see cref="AssemblyLoadFailedBuilderException"/> to <paramref name="exceptions"/>
    /// </summary>
    public static Assembly? TryLoad(this AssemblyName assembly, ExceptionBuilder exceptions)
    {
        try
        {
            return Assembly.Load(assembly);
        }
        catch (Exception e)
        {
            exceptions.Add(new AssemblyLoadFailedBuilderException(assembly, e));
            return null;
        }
    }
    /// <summary>
    /// Tries to load all given <paramref name="assemblies"/> and returns the successfully loaded assemblies.<br/>
    /// Each failed load will be added to <paramref name="exceptions"/> as <see cref="AssemblyLoadFailedBuilderException"/>, which contains a reference to the name in <see cref="AssemblyLoadFailedBuilderException.AssemblyName"/>
    /// </summary>
    public static IEnumerable<Assembly> TryLoadAll(this IEnumerable<AssemblyName> assemblies, ExceptionBuilder exceptions) 
        => assemblies.Select(name => TryLoad(name, exceptions)).WhereNotNull();
}
