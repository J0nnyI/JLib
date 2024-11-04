using System.Reflection;
using AutoMapper.Internal;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataGeneration;

/// <summary>
/// <see cref="DataPackage"/>
/// </summary>
[IsDerivedFrom(typeof(DataPackage)), NotAbstract]
public record DataPackageType(Type Value) : TypeValueType(Value), IValidatedType
{
    /// <summary>
    /// <inheritdoc cref="IValidatedType.Validate"/>
    /// </summary>
    public void Validate(ITypeCache cache, TypeValidationContext value)
    {
        value.ShouldBeSealed("a DataPackage has to be either Sealed or Abstract.");

        value.ValidateProperties(p => p.IsPublic(), p => p
            .HavePublicInit()
            .HavePublicGet());
    }
}

/// <summary>
/// represents a collection of <see cref="DataPackage"/>s
/// </summary>
public abstract class DataPackageCollection : DataPackage
{
    /// <summary>
    /// <inheritdoc cref="DataPackageCollection"/>
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="dependencies"></param>
    protected DataPackageCollection(IServiceProvider provider, IReadOnlyCollection<Type> dependencies) : base(provider)
    {
        foreach (var dependency in dependencies)
        {
            provider.GetRequiredService(dependency);
        }
    }
}

/// <summary>
/// defines a behavior of how the data generation should create data.<br/>
/// a data package should always generate or include all dependencies to be complete.
/// </summary>
public abstract class DataPackage
{
    private readonly IDataPackageManager _packageManager;

    /// <summary>
    /// contains the binding flags which will be used to discover id properties
    /// </summary>
    public const BindingFlags PropertyDiscoveryBindingFlags =
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidSetupException"></exception>
    public string GetInfoText(string propertyName)
    {
        var property = GetType().GetProperty(propertyName, PropertyDiscoveryBindingFlags) ??
                       throw new InvalidSetupException(
                           $"property {propertyName} not found on {GetType().FullName()}");
        var identifier = _packageManager.IdRegistry.ApplyIdentifierPostProcessor(new(property));
        return identifier.ToString();
    }

    protected DataPackage(IServiceProvider provider) : this(provider.GetRequiredService<IDataPackageManager>()) { }
    protected DataPackage(IDataPackageManager packageManager)
    {
        _packageManager = packageManager;
        switch (packageManager.InitState)
        {
            case DataPackageInitState.Uninitialized:
                throw new InvalidOperationException(
                    "invalid injection. inject directly after provider build using 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.");
            case DataPackageInitState.Initialized:
                throw new InvalidOperationException(
                    "invalid injection: this type package has not been include when calling 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.");
            case DataPackageInitState.Initializing:
                break;
            default:
                throw new IndexOutOfRangeException(nameof(packageManager.InitState));
        }

        foreach (var propertyInfo in GetType().GetProperties(PropertyDiscoveryBindingFlags))
            packageManager.SetIdPropertyValue(this, propertyInfo);
    }
}