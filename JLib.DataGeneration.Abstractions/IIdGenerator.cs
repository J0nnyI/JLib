using JLib.ValueTypes;
using ValueType = JLib.ValueTypes.ValueType;

namespace JLib.DataGeneration.Abstractions;

/// <summary>
/// Represents an interface for generating <see cref="Guid"/>s.
/// <remarks><br/>
/// Use <see cref="IdGeneratorServiceCollectionExtensions.AddIdGenerator"/> to provide ID's in a testing environment<br/>
/// Use `DataPackageExtensions.AddTestingIdGenerator` of the `JLib.DataGeneration` package instead while testing.<br/>
/// </remarks>
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// Creates a new <see cref="Guid"/>.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    Guid CreateGuid();

    /// <summary>
    /// Creates a new <see cref="Guid"/> of type <typeparamref name="TVt"/>.
    /// </summary>
    /// <typeparam name="TVt">The type of the <see cref="GuidValueType"/> to create.</typeparam>
    /// <returns>A new <see cref="Guid"/> of type <typeparamref name="TVt"/>.</returns>
    TVt CreateGuid<TVt>()
        where TVt : GuidValueType;
}

/// <summary>
/// Runtime implementation of the <see cref="IIdGenerator"/> interface.<br/>
/// Requires <see cref="IMapper"/>
/// </summary>
public sealed class IdGenerator : IIdGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdGenerator"/> class.
    /// </summary>
    /// <param name="mapper">The <see cref="IMapper"/> instance used for mapping.</param>

    /// <summary>
    /// Creates a new <see cref="Guid"/>.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    public Guid CreateGuid() 
        => Guid.NewGuid();

    /// <summary>
    /// Creates a new <see cref="Guid"/> of type <typeparamref name="TVt"/>.
    /// </summary>
    /// <typeparam name="TVt">The type of the <see cref="GuidValueType"/> to create.</typeparam>
    /// <returns>A new <see cref="Guid"/> of type <typeparamref name="TVt"/>.</returns>
    public TVt CreateGuid<TVt>() where TVt : GuidValueType
        => ValueType.Create<TVt,Guid>(CreateGuid());
}
