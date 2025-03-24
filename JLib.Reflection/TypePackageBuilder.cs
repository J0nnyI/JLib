using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using JLib.Exceptions;
using JLib.Helper;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JLib.Reflection;

/// <summary>
/// This Attribute forces a reference to an Assembly, the types of which may not be referenced otherwise by this assembly.<br/>
/// This is required, when the referencing assembly does use the types of the referenced assembly for reflection but does not reference them directly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class EnforceReferenceToAttribute : Attribute
{
    /// <summary>
    /// <inheritdoc cref="EnforceReferenceToAttribute"/>
    /// </summary>
    /// <param name="type">The Type, which is defined by the assembly </param>
    public EnforceReferenceToAttribute(Type type) { }
}

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

/**********************************************************************************************************
 * extensions for the type package builder:
 * - create a mode, which considers only assemblies which directly or indirectly reference JLib.Reflection
 *   or are explicitly marked via attribute
 * - add from fs path with wildcard filter
 * - add from local executing path with wildcard filter
 * - create a mode, where the dev works directly on the Enumerable of types and assemblies
 * - use a graph to make the assemblies more accessible
 **********************************************************************************************************/
/// <summary>
/// Builds a new <see cref="ITypePackage"/> used to initialize a <see cref="ITypeCache"/>
/// </summary>
public sealed class TypePackageBuilder(ILoggerFactory? loggerFactory = null, TypePackageBuilderOptions? options = null)
{
    private readonly TypePackageBuilderOptions _options = options ?? TypePackageBuilderOptions.Default;
    private readonly HashSet<Assembly> _includedAssemblies = [];
    private readonly HashSet<AssemblyName> _includedAssemblyNames = [];
    private readonly HashSet<Type> _includedTypes = [];
    private readonly HashSet<AssemblyName> _assemblyBlacklist = [];
    private readonly HashSet<Type> _typeBlacklist = [];
    private readonly List<Func<Type, bool>> _typeFilters = [];
    private readonly List<Func<Assembly, bool>> _assemblyFilters = [];

    /// <summary>
    /// Adds the given <paramref name="assemblies"/> to the <see cref="ITypePackage"/>.<br/>
    /// This will also add all recursive peer dependencies of this <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder Add(params Assembly?[] assemblies)
    {
        _includedAssemblies.AddRange(assemblies.WhereNotNull());
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="assemblyNames"/> to the <see cref="ITypePackage"/>.<br/>
    /// This will also add all recursive peer dependencies of this <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder Add(params AssemblyName?[] assemblyNames)
    {
        _includedAssemblyNames.AddRange(assemblyNames.WhereNotNull());
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="types"/> to the <see cref="ITypePackage"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder Add(params Type?[] types)
    {
        _includedTypes.AddRange(types.WhereNotNull());
        return this;
    }

    /// <summary>
    /// the given <paramref name="assemblies"/> will not be included in the resulting type package, whether they are peer or direct references.
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder AddToBlacklist(params Assembly?[] assemblies)
    {
        _assemblyBlacklist.AddRange(assemblies
            .WhereNotNull()
            .Select(a => a.GetName())
        );
        return this;
    }

    /// <summary>
    /// The given <paramref name="assemblies"/> will not be included in the resulting type package, even if it is required by another <see cref="Assembly"/>.<br/>
    /// Note, that it's dependencies will not be evaluated and therefore not loaded if they are not included by another <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder AddToBlacklist(params AssemblyName?[] assemblies)
    {
        _assemblyBlacklist.AddRange(assemblies.WhereNotNull());
        return this;
    }

    /// <summary>
    /// Adds all <paramref name="types"/> to the Blacklist, meaning that they won't be included in the built <see cref="ITypePackage"/>.
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder AddToBlacklist(params Type?[] types)
    {
        _typeBlacklist.AddRange(types.WhereNotNull());
        return this;
    }

    /// <summary>
    /// Applies the given <paramref name="filters"/> to all types of the resulting <see cref="ITypePackage"/>.<br/>
    /// All types which evaluate to <see langword="false"/> on at least one Filter will not be included, independent on whether they have been added manually or via an <see cref="Assembly"/>.
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public TypePackageBuilder AddTypeFilter(params Func<Type, bool>[] filters)
    {
        _typeFilters.AddRange(filters);
        return this;
    }
    /// <summary>
    /// Applies the given <paramref name="filters"/> to all types of the resulting <see cref="ITypePackage"/>.<br/>
    /// All types which evaluate to <see langword="false"/> on at least one Filter will not be included, independent on whether they have been added manually or via an <see cref="Assembly"/>.
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public TypePackageBuilder AddAssemblyFilter(params Func<Assembly, bool>[] filters)
    {
        _assemblyFilters.AddRange(filters);
        return this;
    }

    private const string ExceptionBuilderName = $"{nameof(TypePackageBuilder)}.{nameof(Build)}";

    /// <summary>
    /// creates an immutable <see cref="ITypePackage"/> from this <see cref="TypePackageBuilder"/>.
    /// </summary>
    /// <param name="parentExceptions">the exceptionBuilder to add the exceptions too. The exceptions will be thrown as <see cref="JLibAggregateException"/> if this parameter is <see langword="null"/></param>
    /// <returns>The <see cref="TypePackageBuilder"/> created according to the settings</returns>
    /// <exception cref="JLibAggregateException"></exception>
    public ITypePackage Build(ExceptionBuilder? parentExceptions = null)
    {

        using var exceptions = parentExceptions?.CreateChild(ExceptionBuilderName)
                     ?? new(ExceptionBuilderName);

        var logger = loggerFactory?.CreateLogger<TypePackageBuilder>();
        Dictionary<Assembly, ITypePackage> packagesToInclude = [];

        var peerDependencies = _includedAssemblies
            .Concat(LoadAssemblies(_includedAssemblyNames, exceptions.CreateChild("loading included assembly names")))
            .ToHashSet();

        exceptions.CreateChild("Some Assemblies have been both explicitly included and added to the blacklist",
            _includedAssemblies
                .Where(includedAssembly => _assemblyBlacklist
                    .Any(blacklistAssembly => AssemblyName.ReferenceMatchesDefinition(includedAssembly.GetName(), blacklistAssembly))
                ).Select(name => name.FullName ?? "no name"));

        for (int i = 0; i < _options.MaxDepth && peerDependencies.Count > 0; i++)
        {
            var currentDependencies = peerDependencies.ToArray();
            peerDependencies.Clear();

            foreach (var assembly in ApplyAssemblyFilter(currentDependencies))
            {
                try
                {
                    var assemblyName = assembly.GetName();
                    if (_assemblyBlacklist.Count != 0 && _assemblyBlacklist.Any(name => AssemblyName.ReferenceMatchesDefinition(name, assemblyName)))
                        continue;

                    if (packagesToInclude.Keys.Any(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName)))
                        continue;

                    var typePackage = CreateContentTypePackage(assembly);

                    packagesToInclude.Add(assembly, typePackage);

                    var dependencyNames = LoadAssemblies(assembly
                        .GetReferencedAssemblies(),
                        // this uses loaded assemblies only, and only if the types are referenced directly. 
                        exceptions.CreateChild($"referenced assemblies of {assembly.FullName}"));


                    peerDependencies.AddRange(dependencyNames);
                }
                catch (Exception e)
                {

                    logger?.LogError(e, "could not load assembly {assemblyName}", assembly.FullName);
                    exceptions.Add(new Exception($"could not load assembly {assembly.FullName}", e));
                }
            }

            if (i == _options.MaxDepth - 1)
                exceptions.Add(new InvalidOperationException("Max peer dependency depth exceeded"));
        }

        return new RootTypePackage(ApplyTypeFilter(_includedTypes), packagesToInclude.Values);

        ImmutableHashSet<Type> ApplyTypeFilter(IEnumerable<Type> types)
            => types
                .Except(_typeBlacklist)
                .Where(t
                    // take all if there are no filters
                    => _typeFilters.Count == 0
                        // remove all types where at east one filter returned false
                        || _typeFilters.All(f => f(t))
                )
                .ToImmutableHashSet();

        ImmutableHashSet<Assembly> ApplyAssemblyFilter(IEnumerable<Assembly> assemblies)
            => assemblies
                .Where(a
                    // take all if there are no filters
                    => _assemblyFilters.Count == 0
                        // remove all types where at east one filter returned false
                        || _assemblyFilters.All(f => f(a))
                )
                .ToImmutableHashSet();

        ITypePackage CreateContentTypePackage(Assembly assembly)
        {
            // loading an assembly might fail, for example when nuget packages are incompatible with one another.
            var allTypes = assembly.GetTypes();
            var filteredTypes = ApplyTypeFilter(assembly.GetTypes());

            var name = $"Assembly {assembly.FullName} with ";
            if (allTypes.Length != filteredTypes.Count)
                name += $"{filteredTypes.Count}/{allTypes.Length} types";
            else
                name += $"{filteredTypes.Count} types";

            return new ContentTypePackage(name, filteredTypes);
        }
    }

    private IReadOnlyCollection<Assembly> LoadAssemblies(IReadOnlyCollection<AssemblyName> assemblyNames, ExceptionBuilder parentExceptionBuilder)
    {
        var dependencyNames = assemblyNames
            .Except(_assemblyBlacklist)
            .Select(name =>
            {
                try
                {
                    return new
                    {
                        assembly = (Assembly?)Assembly.Load(name),
                        exception = (Exception?)null,
                        assemblyName = name
                    };
                }
                catch (Exception e)
                {
                    return new
                    {
                        assembly = (Assembly?)null,
                        exception = (Exception?)new TypePackageBuilderException.AssemblyLoadFailedBuilderException(name, e),
                        assemblyName = name
                    };
                }
            }).ToReadOnlyCollection();

        parentExceptionBuilder.CreateChild("Some assemblies could not be loaded", dependencyNames
            .Select(x=>x.exception)
            .WhereNotNull());
        
        return dependencyNames
            .Select(x => x.assembly)
            .WhereNotNull()
            .ToReadOnlyCollection();
    }

    private sealed class RootTypePackage(ImmutableHashSet<Type> types, IReadOnlyCollection<ITypePackage> assemblies) : ITypePackage
    {

        private readonly ImmutableHashSet<Type> _content = types.Concat(assemblies.SelectMany(a => a.GetContent())).ToImmutableHashSet();
        public ImmutableHashSet<Type> GetContent() => _content;
        public IEnumerable<ITypePackage> Children => assemblies;
        IEnumerable<Type> ITypePackage.Types => types;
        public string DescriptionTemplate => $"{types.Count} types and {assemblies.Count} assemblies";
        [Obsolete]
        ITypePackage ITypePackage.Combine(params ITypePackage[] packages)
            => TypePackage.Get(packages.Prepend(this));
    }


    private sealed class ContentTypePackage(string name, ImmutableHashSet<Type> content) : ITypePackage
    {
        public ImmutableHashSet<Type> GetContent() => content;
        IEnumerable<ITypePackage> ITypePackage.Children => [];
        IEnumerable<Type> ITypePackage.Types => content;
        public string DescriptionTemplate { get; } = name;

        [Obsolete]
        ITypePackage ITypePackage.Combine(params ITypePackage[] packages)
            => TypePackage.Get(packages.Prepend(this));
    }
}