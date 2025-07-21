using JLib.ValueTypes;
using ValueType = JLib.ValueTypes.ValueType;

namespace JLib.DataGeneration.Abstractions;

/// <summary>
/// Runtime implementation of the <see cref="IIdGenerator"/> interface.<br/>
/// Requires <see cref="IMapper"/>
/// </summary>
public sealed class IdGenerator : IIdGenerator
{
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
        => ValueType.Create<TVt, Guid>(Guid.NewGuid());
}