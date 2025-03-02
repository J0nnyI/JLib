using System.Collections.Immutable;
using System.Reflection;

using JLib.Exceptions;
using JLib.Helper;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

/// <summary>
/// Builds a new <see cref="ITypePackage"/> used to initialize a <see cref="ITypeCache"/>
/// </summary>
public sealed class TypePackageBuilder(ILoggerFactory? loggerFactory = null, TypePackageBuilderOptions? options = null)
{
    private readonly TypePackageBuilderOptions _options = options ?? TypePackageBuilderOptions.Default;
    private readonly HashSet<Assembly> _includedAssemblies = [];
    private readonly HashSet<Type> _includedTypes = [];
    private readonly HashSet<AssemblyName> _assemblyBlacklist = [];
    private readonly HashSet<Type> _typeBlacklist = [];
    private readonly List<Func<Type, bool>> _typeFilters = [];

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
        var referencesToCheck = _includedAssemblies.ToHashSet();


        for (int i = 0; i < _options.MaxDepth && referencesToCheck.Count > 0; i++)
        {
            var currentReferences = referencesToCheck.ToArray();
            referencesToCheck.Clear();
            foreach (var assemblyName in currentReferences
                         .SelectMany(a => a.GetReferencedAssemblies()))
                LoadAssembly(assemblyName);

            if (i == _options.MaxDepth - 1)
                exceptions.Add(new InvalidOperationException("Max peer dependency depth exceeded"));
        }

        return new RootTypePackage(ApplyFilter(_includedTypes), packagesToInclude.Values);

        ImmutableHashSet<Type> ApplyFilter(IEnumerable<Type> types)
            => types
                .Except(_typeBlacklist)
                .Where(t
                    // take all if there are no filters
                    => _typeFilters.Count == 0
                        // remove all types where at east one filter returned false
                        || _typeFilters.All(f => f(t))
                )
                .ToImmutableHashSet();

        void LoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                if (_assemblyBlacklist.Contains(assemblyName))
                    return;

                var typePackage = CreateContentTypePackage(assemblyName, out var assembly);

                if (packagesToInclude.TryAdd(assembly, typePackage))
                    // if the assembly has already been added, it has also already been checked for peer dependencies.
                    return;

                referencesToCheck.Add(assembly);
            }
            catch (Exception e)
            {

                logger?.LogError(e, "could not load assembly {assemblyName}", assemblyName.FullName);
                exceptions.Add(new Exception($"could not load assembly {assemblyName.FullName}", e));
            }
        }

        ITypePackage CreateContentTypePackage(AssemblyName assemblyName, out Assembly assembly)
        {
            // loading an assembly might fail, for example when nuget packages are incompatible with one another.
            assembly = Assembly.Load(assemblyName);
            var allTypes = assembly.GetTypes();
            var filteredTypes = ApplyFilter(assembly.GetTypes());

            var name = $"Assembly {assembly.FullName} with ";
            if (allTypes.Length != filteredTypes.Count)
                name += $"{filteredTypes.Count}/{allTypes.Length} types";
            else
                name += $"{filteredTypes.Count} types";

            return new ContentTypePackage(name, filteredTypes);
        }
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