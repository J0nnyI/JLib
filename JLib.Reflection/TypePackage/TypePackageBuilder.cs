using System.Collections.Immutable;
using System.Reflection;

using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

using Microsoft.Extensions.Logging;

namespace JLib.Reflection;

/**********************************************************************************************************
 * extensions for the type package builder:
 * - create a mode, which considers only assemblies which directly or indirectly reference JLib.Reflection
 *   or are explicitly marked via attribute
 * - add from fs path with wildcard filter
 * - add from local executing path with wildcard filter
 **********************************************************************************************************/

/*
 Architecture Notes
 Q & A
- Q: Why can't we just load the Assemblies directly while adding them to the Builder?
  A: Their peer dependencies, which we would have to load too, may be excluded later on.
- Q: We may load assemblies twice when adding them not by name but by assembly object. Why don't we optimize that?
  A: Because they use a cache and remain loaded. Loading them again returns the original reference

Requirement Notes
Q & A
- Q: Why would you want to add types and assemblies manually?
  A: For Testing purposes. You may want to be able to load one type, without including the entire assembly.
     This is a common requirement when testing TypeValueTypes.
- Q: Why do we need a blacklist?
  A: Some assemblies in a directory may not be able to be loaded and/or not be needed for reflection.
     Excluding those removes their loading exception from the builder, lets the initialization succeed and shortens the initialization period.
- Q: Why do we need a type filter?
  A: Some types may not be needed or wanted for reflection. 3rd party AutoMapper profiles which do not conform to the TypeValueType Validation are one example.
- Q: Why would you not want to load the peer Dependencies?
  A: Because you don't have to when you are loading the entire binary directory, or you don't want to because you are setting up a test environment
- Q: Why would you want to load the peer dependencies?
  A: Because you want to include a Package, like any JLib library, and they do need their peer dependencies in the TypeCache.
 */

/// <summary>
/// Builds a new <see cref="ITypePackage"/> used to initialize a <see cref="ITypeCache"/>
/// </summary>
/// <seealso cref="TypePackageBuilderExtensions"/>
public sealed class TypePackageBuilder(ILoggerFactory? loggerFactory = null, TypePackageBuilderOptions? options = null)
{
    private record AssemblyFullName(string? Value) : StringValueType(Value ?? "n/a");

    private sealed class AssemblyLoadInfo(AssemblyName assemblyName, AssemblyLoadMode mode)
    {

        public AssemblyName Name { get; } = assemblyName;
        public AssemblyFullName FullName => new(Name.FullName);
        public AssemblyLoadMode Mode { get; } = mode;
    }

    private readonly TypePackageBuilderOptions _options = options ?? TypePackageBuilderOptions.Default;

    private readonly Dictionary<AssemblyFullName, AssemblyLoadInfo> _includedAssemblyNames = [];

    private readonly HashSet<Type> _includedTypes = [];
    private readonly HashSet<AssemblyFullName> _assemblyBlacklist = [];
    private readonly HashSet<Type> _typeBlacklist = [];
    private readonly List<Func<Type, bool>> _typeFilters = [];
    private readonly List<Func<Assembly, bool>> _assemblyFilters = [];

    private const string ExceptionBuilderName = $"{nameof(TypePackageBuilder)}.{nameof(Build)}";

    /// <summary>
    /// creates an immutable <see cref="ITypePackage"/> from this <see cref="TypePackageBuilder"/>.
    /// </summary>
    /// <param name="parentExceptions">the exceptionBuilder to add the exceptions too. The exceptions will be thrown as <see cref="JLibAggregateException"/> if this parameter is <see langword="null"/></param>
    /// <returns>The <see cref="TypePackageBuilder"/> created according to the settings</returns>
    /// <exception cref="JLibAggregateException"></exception>
    public ITypePackage Build(ExceptionBuilder? parentExceptions = null)
    {
        var content = new List<ITypePackage>();
        using var exceptions = parentExceptions?.CreateChild(ExceptionBuilderName)
                     ?? new(ExceptionBuilderName);

        var logger = loggerFactory?.CreateLogger<TypePackageBuilder>();

        exceptions.CreateChild("Some Assemblies have been both explicitly included and added to the blacklist",
            _includedAssemblyNames
                .Where(includedAssembly => _assemblyBlacklist.Contains(includedAssembly.Key))
                .Select(kv => kv.Key.Value));

        content.Add(new ContentTypePackage($"{_includedTypes.Count} Manually added types", _includedTypes.ToImmutableHashSet()));

        logger?.LogTrace("preparing direct dependencies");
        var loadDependencyExceptions = Enum.GetValues<AssemblyLoadMode>()
            .ToDictionary(value => value, value => exceptions.CreateChild(value.ToString()));

        var loadGroups = _includedAssemblyNames
            .Select(kv => kv.Value)
            .GroupBy(
                loadInfo => loadInfo.Mode,
                loadInfo => new
                {
                    loadInfo,
                    // this loads the direct dependencies
                    assembly = loadInfo.Name.TryLoad(loadDependencyExceptions[loadInfo.Mode])
                })
            .ToDictionary(
                group => group.Key,
                group => group.ToReadOnlyCollection());


        logger?.LogTrace("adding direct dependencies");
        content.AddRange(loadGroups
            .Select(
                // grouped by assembly load mode
                kv => new TypePackageCollection($"{kv.Value.Count} {kv.Key.ToString()} Assemblies",
                    kv.Value
                        // convert the assembly to a type package
                        .Select(assembly => new ContentTypePackage(assembly.loadInfo.FullName, assembly.assembly, ApplyTypeFilter))
                        .ToReadOnlyCollection()
                )
            ));

        logger?.LogTrace("adding peer dependencies");
        var peerDependencies = (loadGroups
             .TryGetValue(AssemblyLoadMode.Recursive)
             ?.Select(assembly => assembly.assembly)
             .WhereNotNull()
             .LoadRecursivePeerDependencies(
                 exceptions.CreateChild("peer dependencies"),
                 name => _assemblyBlacklist.Count == 0 || _assemblyBlacklist.Contains(new(name.FullName)),
                 _options.MaxDepth)
             .WhereNotNull()
             // remove blacklisted assemblies
             .Where(assembly
                 // take all if there are no filters
                 => _assemblyFilters.Count == 0
                     // remove all types where at east one filter returned false
                     || _assemblyFilters.All(filter => filter(assembly)))
             .Select(assembly => new ContentTypePackage(new(assembly.FullName), assembly, ApplyTypeFilter))
             .ToReadOnlyCollection()
         ?? []);
        content.Add(new TypePackageCollection($"{peerDependencies.Count} peer dependencies", peerDependencies));

        logger?.LogTrace("building the root type package");
        return new TypePackageCollection($"""
                                           {nameof(TypePackageBuilder)} result
                                           {_includedTypes.Count} Manually added types"
                                           {loadGroups.TryGetValue(AssemblyLoadMode.TopLevelOnly)?.Count ?? 0} top level only assemblies,
                                           {loadGroups.TryGetValue(AssemblyLoadMode.Recursive)?.Count ?? 0} recursively loaded assemblies,
                                           {peerDependencies.Count} peer dependencies
                                           """, content);

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
    }

    #region setup methods
    #region add
    /// <summary>
    /// Adds the given <paramref name="assemblies"/> to the <see cref="ITypePackage"/>.<br/>
    /// This will also add all recursive peer dependencies of this <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder Add(AssemblyLoadMode loadMode, params Assembly?[] assemblies)
    {
        _includedAssemblyNames.AddRange(
            assemblies.WhereNotNull().Select(a => new AssemblyLoadInfo(a.GetName(), loadMode)),
            x => x.FullName);
        return this;
    }

    /// <summary>
    /// Adds the given <paramref name="assemblyNames"/> to the <see cref="ITypePackage"/>.<br/>
    /// This will also add all recursive peer dependencies of this <see cref="Assembly"/>
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder Add(AssemblyLoadMode loadMode, params AssemblyName?[] assemblyNames)
    {
        _includedAssemblyNames.AddRange(assemblyNames.WhereNotNull().Select(x => new AssemblyLoadInfo(x, loadMode)), x => x.FullName);
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

    #endregion
    #region blacklist
    /// <summary>
    /// the given <paramref name="assemblies"/> will not be included in the resulting type package, whether they are peer or direct references.
    /// </summary>
    /// <returns><see langword="this"/> instance</returns>
    public TypePackageBuilder AddToBlacklist(params Assembly?[] assemblies)
    {
        _assemblyBlacklist.AddRange(assemblies
            .WhereNotNull()
            .Select(a => new AssemblyFullName(a.FullName))
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
        _assemblyBlacklist.AddRange(assemblies.WhereNotNull().Select(a => new AssemblyFullName(a.FullName)));
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
    #endregion
    #region filter
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
    /// Applies the given <paramref name="filters"/> to all assemblies of the resulting <see cref="ITypePackage"/>.<br/>
    /// All assemblies which evaluate to <see langword="false"/> on at least one Filter will not be included, independent on whether they have been added manually or via an <see cref="Assembly"/>.<br/>
    /// Their peer dependencies will not be loaded either.
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public TypePackageBuilder AddAssemblyFilter(params Func<Assembly, bool>[] filters)
    {
        _assemblyFilters.AddRange(filters);
        return this;
    }
    #endregion
    #endregion

    #region type package classes

    private sealed class TypePackageCollection(string name, IReadOnlyCollection<ITypePackage> containedTypePackages)
        : ITypePackage
    {

        private readonly Lazy<ImmutableHashSet<Type>> _content = new(() => containedTypePackages.SelectMany(c => c.GetContent()).ToImmutableHashSet());
        ImmutableHashSet<Type> ITypePackage.GetContent() => _content.Value;
        public IEnumerable<ITypePackage> Children => containedTypePackages;
        IEnumerable<Type> ITypePackage.Types => [];
        public string DescriptionTemplate => name;
        [Obsolete]
        ITypePackage ITypePackage.Combine(params ITypePackage[] packages)
            => TypePackage.Get(packages.Prepend(this));
    }

    /// <summary>
    /// This package provides types as content without further nesting
    /// </summary>
    private sealed class ContentTypePackage(string name, ImmutableHashSet<Type> content) : ITypePackage
    {
        public ContentTypePackage(AssemblyFullName assemblyName, Assembly? assembly, Func<IEnumerable<Type>, ImmutableHashSet<Type>> applyTypeFilter) : this(assemblyName.Value, applyTypeFilter(assembly?.GetTypes() ?? []))
        {
        }
        public ImmutableHashSet<Type> GetContent() => content;
        IEnumerable<ITypePackage> ITypePackage.Children => [];
        IEnumerable<Type> ITypePackage.Types => content;
        public string DescriptionTemplate { get; } = name;

        [Obsolete]
        ITypePackage ITypePackage.Combine(params ITypePackage[] packages)
            => TypePackage.Get(packages.Prepend(this));
    }
    #endregion
}