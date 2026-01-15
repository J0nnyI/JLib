using System.Collections.Immutable;
using System.Reflection;

using JLib.Exceptions;
using JLib.Exceptions.CommonExceptions;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// Extension Methods for loading Assemblies
/// </summary>
public static class AssemblyLoaderExtensions
{
    /// <summary>
    /// tries to load the given <paramref name="assembly"/>. Returns <see langword="null"/> if the load failed and adds a <see cref="AssemblyLoadFailedException"/> to <paramref name="exceptions"/>
    /// </summary>
    public static Assembly? TryLoad(this AssemblyName assembly, ExceptionBuilder exceptions)
    {
        try
        {
            return Assembly.Load(assembly);
        }
        catch (Exception e)
        {
            exceptions.Add(new AssemblyLoadFailedException(assembly, e));
            return null;
        }
    }
    /// <summary>
    /// Tries to load all given <paramref name="assemblies"/> and returns the successfully loaded assemblies.<br/>
    /// Each failed load will be added to <paramref name="exceptions"/> as <see cref="AssemblyLoadFailedException"/>, which contains a reference to the name in <see cref="AssemblyLoadFailedException.AssemblyName"/>
    /// </summary>
    public static IEnumerable<Assembly> TryLoadAll(this IEnumerable<AssemblyName> assemblies, ExceptionBuilder exceptions)
        => assemblies.Select(name => TryLoad(name, exceptions)).WhereNotNull();

    /// <summary>
    /// returns all recursive dependencies of the given <paramref name="assemblies"/>. This includes all assemblies that are referenced by the given assemblies and their dependencies but not <paramref name="assemblies"/>
    /// </summary>
    public static ImmutableHashSet<Assembly> LoadRecursivePeerDependencies(
        this IEnumerable<Assembly> assemblies,
        ExceptionBuilder exceptions,
        Func<AssemblyName, bool> filter,
        int maxDependencyDepth = 1000
    )
    => LoadRecursivePeerDependencies(assemblies.Select(x => x.GetName()).ToReadOnlyCollection(), filter, exceptions, maxDependencyDepth);
    /// <summary>
    /// returns all recursive dependencies of the given <paramref name="assemblies"/>. This includes all assemblies that are referenced by the given assemblies and their dependencies but not <paramref name="assemblies"/>
    /// </summary>
    public static ImmutableHashSet<Assembly> LoadRecursivePeerDependencies(
        this IReadOnlyCollection<AssemblyName> assemblies,
        ExceptionBuilder exceptions,
        int maxDependencyDepth = 1000
        )
        => LoadRecursivePeerDependencies(assemblies, _ => true, exceptions, maxDependencyDepth);

    /// <summary>
    /// returns all recursive dependencies of the given <paramref name="assemblies"/>. This includes all assemblies that are referenced by the given assemblies and their dependencies but not <paramref name="assemblies"/><br/>
    /// No <see cref="Exception"/>s will be <see langword="throw"/>n, but added to <paramref name="exceptions"/>.
    /// </summary>
    /// <param name="assemblies">The <see cref="AssemblyName"/>s of which you want to load the peer dependencies</param>
    /// <param name="filter"><see cref="AssemblyName"/>s, which evaluate to false, will not be loaded and their peer dependencies will not be included.</param>
    /// <param name="exceptions">an <see cref="ExceptionBuilder"/> where a child with <see cref="AssemblyLoadFailedException"/>s will be added for each <see cref="Assembly"/> which could not be loaded</param>
    /// <param name="maxDependencyDepth">the maximum depth of the dependency tree.</param>
    public static ImmutableHashSet<Assembly> LoadRecursivePeerDependencies(
        this IReadOnlyCollection<AssemblyName> assemblies,
        Func<AssemblyName, bool> filter,
        ExceptionBuilder exceptions,
        int maxDependencyDepth = 1000
        )
    {
        exceptions = exceptions.CreateChild("Loading Peer Dependencies");
        var peerDependencies = new Dictionary<string, Assembly>();
        HashSet<AssemblyName>? nextLoadLevel = null;

        for (var dependencyDepth = 0;
             nextLoadLevel is null || nextLoadLevel.Count > 0;
             dependencyDepth++)
        {
            if (dependencyDepth >= maxDependencyDepth)
            {
                exceptions.Add(new MaxIterationDepthReachedException(maxDependencyDepth));
                break;
            }

            var currentLoadLevel = nextLoadLevel?.ToImmutableHashSet() ?? assemblies.ToImmutableHashSet();
            nextLoadLevel = [];

            foreach (var assemblyName in currentLoadLevel)
            {
                if (peerDependencies.ContainsKey(assemblyName.FullName) // has already been handled
                    || filter?.Invoke(assemblyName) is false) // has been excluded by filter
                    continue;

                var assembly = assemblyName.TryLoad(exceptions);

                if (assembly is null)// load failed
                    continue;

                peerDependencies.Add(assemblyName.FullName, assembly);

                nextLoadLevel.AddRange(assembly.GetReferencedAssemblies());
            }

        }


        peerDependencies.RemoveRange(assemblies.Select(x => x.FullName));

        exceptions.Dispose();// remove the child from the parent if no exceptions have been added

        return peerDependencies.Values.ToImmutableHashSet();
    }
}
