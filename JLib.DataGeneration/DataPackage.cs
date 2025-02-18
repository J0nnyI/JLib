using System.Reflection;

using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;

using Microsoft.Extensions.DependencyInjection;

using static JLib.DataGeneration.DataPackageException.InitializationException.InvalidAccessException;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataGeneration;

/// <summary>
/// <see cref="TypeValueType"/> for <see cref="DataPackage"/>s
/// </summary>
[IsDerivedFrom(typeof(DataPackage)), NotAbstract]
public record DataPackageType : TypeValueType, IValidatedType
{
    internal IReadOnlyCollection<PropertyInfo> IdProperties { get; }

    internal const BindingFlags PropertyDiscoveryBindingFlags = BindingFlags.Instance | BindingFlags.Public;
    /// <summary>
    /// <inheritdoc cref="DataPackageType"/>
    /// </summary>
    public DataPackageType(Type value) : base(value)
    {
        IdProperties = value.GetProperties(PropertyDiscoveryBindingFlags)
            .Where(x =>
                x.HasCustomAttribute<SkipIdAssignmentAttribute>() is false
            ).ToReadOnlyCollection();
    }
    void IValidatedType.Validate(ITypeCache cache, IValidationContext<Type> value)
    {
        value.ShouldBeSealed("a DataPackage has to be either Sealed or Abstract.");

        value.ValidateProperties(p => IdProperties.Contains(p), p => p
            .HavePublicInit()
            .HavePublicGet());

        value.AddSubValidators(
            new ExceptionBuilder("duplicate properties found",
                IdProperties.GroupBy(x => x.Name)
                    .Where(x => x.Count() > 1)
                    .Select(x =>
                        new AggregateException($"property {x.Key} is defined multiple times.",
                            x.Select(y =>
                                new Exception(y.ToDebugInfo())
                            )
                        )
                    )
                    .ToArray<Exception>()
                ));

    }
}

/// <summary>
/// may be used inside a <see cref="DataPackage"/> to skip the assignment of the id property.
/// This may be used to create public, non id properties
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SkipIdAssignmentAttribute : Attribute { }

/// <summary>
/// Generates persistent, unique IDs and resolves dependencies using Dependency Injection.
/// </summary>
public abstract class DataPackage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataPackageManager _packageManager;

    internal DataPackageValues.IdIdentifier IdentifierOfIdProperty(PropertyInfo prop)
    => new(prop, _packageManager.Configuration);


    /// <summary>
    /// <inheritdoc cref="DataPackage"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="PreInitializationInstantiationException"></exception>
    /// <exception cref="PostInitializationAccessException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
    protected DataPackage(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _packageManager = serviceProvider.GetRequiredService<DataPackageManager>();
        switch (_packageManager.InitState)
        {
            case DataPackageInitState.Uninitialized:
                throw new PreInitializationInstantiationException(this);
            case DataPackageInitState.Initialized:
                throw new PostInitializationAccessException(this);
            case DataPackageInitState.Initializing:
                break;
            default:
                throw new InvalidInitStateException(this, _packageManager.InitState);
        }

        var typeCache = serviceProvider.GetRequiredService<ITypeCache>();
        var tvt = typeCache.Get<DataPackageType>(GetType());

        foreach (var propertyInfo in tvt.IdProperties)
            _packageManager.SetIdPropertyValue(this, propertyInfo);
    }


    #region IncludeDataPackage
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    protected void Include(params Type[] dataPackages)
        => _serviceProvider.GetRequiredServices(dataPackages);

    //The following methods have been generated using this code:
    //var sb = new StringBuilder();
    //for (int i = 1; i <= 20; i++)
    //{
    //    sb.AppendLine("    /// <summary>")
    //        .AppendLine("    /// loads the given <see cref=\"DataPackage\"/>. should only be called inside the <see cref=\"DataPackage\"/> ctor.")
    //        .AppendLine("    /// </summary>")
    //        .Append("    public void IncludeDataPackages<").AppendJoin(", ", Enumerable.Range(1, i).Select(i => $"TDp{i}")).AppendLine(">()")
    //        .AppendJoin(Environment.NewLine, Enumerable.Range(1, i).Select(i => $"        where TDp{i} : DataPackage")).AppendLine()
    //        .AppendLine($"        => _serviceProvider.IncludeDataPackages(")
    //        .AppendJoin("," + Environment.NewLine, Enumerable.Range(1, i).Select(i => $"            typeof(TDp{i})")).AppendLine()
    //        .AppendLine("        );");
    //}
    //Console.WriteLine(sb)

    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1>()
        where TDp1 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14),
            typeof(TDp15)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14),
            typeof(TDp15),
            typeof(TDp16)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14),
            typeof(TDp15),
            typeof(TDp16),
            typeof(TDp17)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
        where TDp18 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14),
            typeof(TDp15),
            typeof(TDp16),
            typeof(TDp17),
            typeof(TDp18)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18, TDp19>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
        where TDp18 : DataPackage
        where TDp19 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14),
            typeof(TDp15),
            typeof(TDp16),
            typeof(TDp17),
            typeof(TDp18),
            typeof(TDp19)
        );
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be called inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public void IncludeDataPackages<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18, TDp19, TDp20>()
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
        where TDp18 : DataPackage
        where TDp19 : DataPackage
        where TDp20 : DataPackage
        => _serviceProvider.IncludeDataPackages(
            typeof(TDp1),
            typeof(TDp2),
            typeof(TDp3),
            typeof(TDp4),
            typeof(TDp5),
            typeof(TDp6),
            typeof(TDp7),
            typeof(TDp8),
            typeof(TDp9),
            typeof(TDp10),
            typeof(TDp11),
            typeof(TDp12),
            typeof(TDp13),
            typeof(TDp14),
            typeof(TDp15),
            typeof(TDp16),
            typeof(TDp17),
            typeof(TDp18),
            typeof(TDp19),
            typeof(TDp20)
        );


    #endregion
}