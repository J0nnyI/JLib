using System.Reflection;

using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

using Microsoft.Extensions.Logging;

namespace JLib.Reflection;

/// <summary>
/// groups <see cref="Type"/>s by <see cref="TypeValueType"/>, validates them and initializes Navigation
/// <br/> Service interface for the <see cref="TypeCache"/>.
/// <br/>- <seealso cref="TypeValueType"/>
/// <br/>- <seealso cref="NavigatingTypeValueType"/>
/// <br/>- <seealso cref="IValidatedType"/>
/// <br/>- <seealso cref="IPostNavigationInitializedType"/>
/// <br/>- <seealso cref="TvtFactoryAttribute"/>
/// <br/>- <seealso cref="IgnoreInCache"/>
/// </summary>
public interface ITypeCache
{
    /// <summary>
    /// the <see cref="Type"/>s of all known <see cref="TypeValueType"/>s (not their instances)
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypeValueTypes { get; }

    /// <summary>
    /// all types known to the typeCache without any filters or applied valueTypes
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypes { get; }

    /// <returns>the single instance of <typeparamref name="TTvt"/> which satisfies the given <paramref name="filter"/></returns>
    /// <exception cref="TypeValueTypeMismatchException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeException{TRequestedTypeValueType}"></exception>
    /// <exception cref="TypeNotFoundException{TRequestedTypeValueType}"></exception>
    /// <exception cref="NotUniqueTypeFilterException{TRequestedTypeValueType}"></exception>
    public TTvt Get<TTvt>(Func<TTvt, bool> filter) where TTvt : class, ITypeValueType
    {
        var items = All<TTvt>().Where(filter).ToReadOnlyCollection();
        return items.Count switch
        {
            1 => items.Single(),
            > 1 => throw new NotUniqueTypeFilterException<TTvt>(),
            0 => throw new TypeNotFoundException<TTvt>(),
            _ => throw new IndexOutOfRangeException("a negative index is impossible")
        };
    }

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <paramref name="weakType"/></returns>
    /// <exception cref="TypeValueTypeMismatchException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeException{TRequestedTypeValueType}"></exception>
    /// <exception cref="TypeNotFoundException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeValueTypeException{TRequestedTypeValueType}"></exception>
    public TTvt Get<TTvt>(Type weakType) where TTvt : class, ITypeValueType;

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <typeparamref name="TType"/></returns>
    /// <exception cref="TypeValueTypeMismatchException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeException{TRequestedTypeValueType}"></exception>
    /// <exception cref="TypeNotFoundException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeValueTypeException{TRequestedTypeValueType}"></exception>
    public TTvt Get<TTvt, TType>() where TTvt : class, ITypeValueType
        => Get<TTvt>(typeof(TType));

    /// <returns>the single instance of <typeparamref name="TTvt"/> which satisfies the given <paramref name="filter"/></returns>
    /// <exception cref="NotUniqueTypeFilterException{TRequestedTypeValueType}"></exception>
    public TTvt? TryGet<TTvt>(Func<TTvt, bool> filter) where TTvt : class, ITypeValueType
    {
        var res = All<TTvt>().Where(filter).ToReadOnlyCollection();
        return res.Count switch
        {
            1 => res.Single(),
            > 1 => throw new NotUniqueTypeFilterException<TTvt>(),
            0 => null,
            _ => throw new IndexOutOfRangeException("a negative index is impossible")
        };
    }

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <paramref name="weakType"/></returns>
    public TTvt? TryGet<TTvt>(Type? weakType) where TTvt : class, ITypeValueType;

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <typeparamref name="TType"/></returns>
    public TTvt? TryGet<TTvt, TType>() where TTvt : class, ITypeValueType
        => TryGet<TTvt>(typeof(TType).TryGetGenericTypeDefinition());
    /// <returns>All <see cref="TypeValueType"/>s assignable to <typeparamref name="TTvt"/></returns>
    public IEnumerable<TTvt> All<TTvt>() where TTvt : class, ITypeValueType;

    /// <summary>
    /// The <see cref="ITypePackage"/> which was used to create this <see cref="ITypeCache"/>
    /// </summary>
    public ITypePackage TypePackage { get; }
}

/// <summary>
/// provides an easy-to-use way to group types by kind, i.e. entities
/// <br/>searches the Application for <see cref="TypeValueType"/> instances with <see cref="ITypeValueTypeFilter"/> attributes
/// and populates them with the types provided via constructor.
/// <br/> all reflection is done in the constructor
/// <br/> should be used as singleton
/// </summary>
public class TypeCache : ITypeCache
{
    private record ValueTypeForTypeValueTypes : ValueType<Type>
    {
        public ValueTypeForTypeValueTypes(Type Value) : base(Value)
        {
            if (!Value.IsAssignableTo(typeof(TypeValueType)))
                throw new InvalidSetupException($"{Value.Name} does not derive from {nameof(TypeValueType)}");
            if (Value.IsAbstract)
                throw new InvalidSetupException($"{Value.Name} is abstract");
        }

        public bool Filter(Type otherType)
            => Value.GetCustomAttributes()
                .OfType<TvtFactoryAttribute>()
                .All(filterAttr => filterAttr.Filter(otherType));

        public TypeValueType Create(Type type)
        {
            var ctor = Value.GetConstructor(new[] { typeof(Type) })
                       ?? throw new InvalidTypeException(Value, Value, $"ctor not found for {Value.Name}");
            var instance = ctor.Invoke(new object[] { type })
                           ?? throw new InvalidSetupException($"ctor could not be invoked for {Value.Name}");
            return instance as TypeValueType
                   ?? throw new InvalidSetupException($"instance of {Value} is not a {nameof(TypeValueType)}");
        }
    }

    private readonly object _cacheAddLock = new();
    private readonly List<TypeValueType> _typeValueTypes;
    private readonly Dictionary<Type, TypeValueType> _typeMappings;
    private readonly ILogger _logger;

    /// <summary>
    /// <inheritdoc cref="ITypeCache.KnownTypeValueTypes"/>
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypeValueTypes { get; }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.KnownTypes"/>
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypes { get; }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.TypePackage"/>
    /// </summary>
    public ITypePackage TypePackage { get; }

    #region constructor
    /// <summary>
    /// creates an instance of <see cref="TypeCache"/> and initializes all <see cref="TypeValueType"/>s
    /// </summary>
    /// <param name="typePackage"></param>
    /// <param name="parentExceptionManager"></param>
    /// <param name="loggerFactory"></param>
    public TypeCache(ITypePackage typePackage, ExceptionBuilder parentExceptionManager, ILoggerFactory loggerFactory)
    {
        TypePackage = typePackage;
        _logger = loggerFactory.CreateLogger(typeof(ITypeCache).FullName ?? nameof(ITypeCache));
        using var _ = _logger.BeginScope(this);
        KnownTypes = typePackage.GetContent().ToArray();
        const string exceptionMessage = "Cache setup failed";
        using var exceptions = parentExceptionManager.CreateChild(exceptionMessage);

        var availableTypeValueTypes = KnownTypes
            .Where(type => !type.HasCustomAttribute<IgnoreInCache>())
            .Where(type => type.IsAssignableTo<TypeValueType>() && !type.IsAbstract)
            .Select(tvt => new ValueTypeForTypeValueTypes(tvt))
            .ToArray();
        KnownTypeValueTypes = availableTypeValueTypes.Select(tvtt => tvtt.Value).ToArray();

        exceptions.CreateChild(
            "some Types have no filter attributes",
            availableTypeValueTypes.Where(tvtt => tvtt.Value
                .CustomAttributes.None(a =>
                    a.AttributeType.IsAssignableTo<TvtFactoryAttribute>())
            ).Select(tvtt => new InvalidTypeException(tvtt.GetType(), tvtt.Value,
                tvtt.Value.FullName(true)))
        );
        var discoveryExceptions = exceptions.CreateChild("type discovery failed");
        try
        {
            _typeValueTypes = KnownTypes
                .Where(type => !type.HasCustomAttribute<IgnoreInCache>() && !type.IsAssignableTo<TypeValueType>())
                .Select(type =>
                {
                    try
                    {
                        var validTvtGroups = availableTypeValueTypes
                            .Where(availableTvtt => availableTvtt.Filter(type))
                            .ToLookup(t =>
                                t.Value.GetCustomAttribute<TvtFactoryAttribute.PriorityAttribute>()?.Value
                                ?? TvtFactoryAttribute.PriorityAttribute.DefaultPriority);
                        var validTvts = validTvtGroups.MinBy(x => x.Key)?
                            .ToArray() ?? Array.Empty<ValueTypeForTypeValueTypes>();
                        switch (validTvts.Length)
                        {
                            case > 1:
                                discoveryExceptions.Add(new InvalidSetupException(
                                    $"multiple tvt candidates found for type {type.Name} : " +
                                    $@"[ {string.Join(", ", validTvts.Select(tvt =>
                                    {
                                        var priority = tvt.Value.GetCustomAttribute<TvtFactoryAttribute.PriorityAttribute>()?.Value
                                                       ?? TvtFactoryAttribute.PriorityAttribute.DefaultPriority;
                                        return $"{tvt.Value.Name}(priority {priority})";
                                    }).OrderBy(d => d))} ]"));
                                return null;
                            case 0:
                                return null;
                            default:
                                return validTvts.Single().Create(type);
                        }
                    }
                    catch (Exception e)
                    {
                        discoveryExceptions.Add(e);
                        return null;
                    }
                }).WhereNotNull()
                .ToList();

            _typeMappings = _typeValueTypes.ToDictionary(tvt => tvt.Value);
        }
        catch (Exception ex)
        {
            discoveryExceptions.Add(ex);
            if (_typeValueTypes is null || _typeMappings is null)
                throw exceptions.GetException()!;
        }

        // all the following steps have to be done in Get<>() to in case a generic type is requested
        foreach (var typeValueType in _typeValueTypes.OfType<NavigatingTypeValueType>())
        {
            try
            {
                typeValueType.SetCache(this);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        foreach (var typeValueType in _typeValueTypes.OfType<NavigatingTypeValueType>())
        {
            try
            {
                typeValueType.MaterializeNavigation();
            }
            catch (TargetInvocationException e) when (e.InnerException is not null)
            {
                exceptions.Add(e.InnerException);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        foreach (var typeValueType in _typeValueTypes.OfType<IPostNavigationInitializedType>())
        {
            try
            {
                typeValueType.Initialize(this, exceptions.CreateChild("Initialization failed"));
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        foreach (var typeValueType in _typeValueTypes.OfType<IValidatedType>())
        {
            try
            {
                var tvtValidator = new TypeValidationContext(typeValueType.CastTo<TypeValueType>(),
                    typeValueType.GetType());
                typeValueType.Validate(this, tvtValidator);
                exceptions.AddChild(tvtValidator);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        WriteLog();
    }

    #endregion

    private TypeValueType CreateAndGetGenericType<T>(Type weakType)
        where T : class, ITypeValueType
    {
        lock (_cacheAddLock)
        {
            // check if another thread has already added the type
            var strongType = _typeMappings.GetValueOrDefault(weakType);

            if (strongType is not null)
                return strongType;
            var typeDef = weakType.GetGenericTypeDefinition();
            var genericTypeValueType = Get<T>(typeDef);
            var tvtt = new ValueTypeForTypeValueTypes(genericTypeValueType.GetType());
            strongType = tvtt.Create(weakType);
            var exceptions = new ExceptionBuilder(
                $"Deriving {weakType.FullName()} as {genericTypeValueType.Value.FullName()} while {typeof(T).FullName()} is being requested");
            if (strongType is NavigatingTypeValueType navType)
            {
                try
                {
                    navType.SetCache(this);
                    navType.MaterializeNavigation();
                }
                catch (TargetInvocationException e) when (e.InnerException is not null)
                {
                    exceptions.Add(e.InnerException);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            if (strongType is IPostNavigationInitializedType postInit)
                postInit.Initialize(this, exceptions);
            if (strongType is IValidatedType validatedType)
            {
                var validationContext = new TypeValidationContext(strongType, weakType);
                exceptions.AddChild(validationContext);
                validatedType.Validate(this, validationContext);
            }
            exceptions.ThrowIfNotEmpty();
            if (strongType is null)
                throw new();
            _typeValueTypes.Add(strongType);
            _typeMappings.Add(weakType, strongType);

            return strongType;
        }
    }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.Get{TTvt}(System.Type)"/>
    /// </summary>
    public T Get<T>(Type weakType)
        where T : class, ITypeValueType
    {

        var strongType = _typeMappings.GetValueOrDefault(weakType);

        // generic types can only be resolved at runtime and therefore must be created when they are requested but not cached yet
        if (strongType is null && weakType is { IsGenericType: true, IsGenericTypeDefinition: false })
        {
            strongType = CreateAndGetGenericType<T>(weakType);
            return strongType as T ?? throw new TypeValueTypeMismatchException<T>(strongType.GetType(), weakType);
        }

        if (strongType is not null)
            return strongType as T ?? throw new TypeValueTypeMismatchException<T>(strongType.GetType(), weakType);

        if (KnownTypes.Contains(weakType) is false)
            throw new TypeNotFoundException<T>(weakType);
        if (KnownTypeValueTypes.Contains(typeof(T)) is false)
            throw new UnknownTypeValueTypeException<T>(weakType);
        throw new UnknownTypeException<T>(weakType);

    }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.TryGet{TTvt}(System.Type?)"/>
    /// </summary>
    public T? TryGet<T>(Type? weakType)
        where T : class, ITypeValueType
        => weakType is null
            ? null
            : _typeMappings.TryGetValue(weakType, out var tvt)
                ? tvt.As<T?>()
                : null;

    /// <summary>
    /// <inheritdoc cref="ITypeCache.All{TTvt}"/>
    /// </summary>
    public IEnumerable<T> All<T>()
        where T : class, ITypeValueType
        => _typeValueTypes.OfType<T>();

    /// <summary>
    /// writes the contents of the <see cref="TypeCache"/> to the <see cref="ILogger"/>
    /// </summary>
    public void WriteLog()
    {
        using var _ = _logger.BeginScope(this);
        _logger.LogInformation("Initialized TypeCache with a total of {typeCount} types", _typeValueTypes.Count);
        WriteDebug();

        var missing = KnownTypeValueTypes
            .Except(_typeValueTypes.Select(x => x.GetType()).Distinct())
            .ToArray<object>();
        if (missing.Any())
            _logger.LogWarning("  No types found for: {TypeValueTypeName}", missing);
        return;

        void WriteDebug()
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
                return;

            var typesByAssembly = _typeValueTypes
                .ToLookup(tvt => tvt.Value.Assembly.FullName)
                .OrderBy(g => g.Key)
                .ToArray();

            foreach (var typesInAssembly in typesByAssembly)
            {
                _logger.LogDebug("  Found {typeCount} types in Assemlby {assemblyName}", typesInAssembly.Count(),
                    typesInAssembly.Key);
                WriteTypes(typesInAssembly);
            }
            //Log.Verbose("  Total Types:");
            //WriteTypes(_typeValueTypes);
        }

        void WriteTypes(IEnumerable<TypeValueType> types)
        {
            var registeredTypes = types
                .ToLookup(tvt => tvt.GetType())
                .OrderBy(g => g.Key.Name)
                .ToArray();
            foreach (var group in registeredTypes)
            {
                _logger.LogDebug("    ValueTypeType     + {TypeValueTypeName}", group.Key);

                if (!_logger.IsEnabled(LogLevel.Trace))
                    continue;
                foreach (var tvt in group)
                    _logger.LogTrace("      DiscoveredType    - {TypeName}", tvt.Name);
            }
        }
    }
}
