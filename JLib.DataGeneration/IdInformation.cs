using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.DataGeneration;

/// <summary>
/// Bundles Information about the Properties of an entity id
/// </summary>
/// <param name="Type">The <see cref="Type"/> of the <see cref="ValueType{T}"/> used.</param>
/// <param name="Identifier">The <see cref="DataPackageValues.IdIdentifier"/> of the given <paramref name="Value"/></param>
/// <param name="Value">The <see cref="ValueType{T}.Value"/> of the Id</param>
public record struct IdInformation(Type Type, DataPackageValues.IdIdentifier Identifier, object? Value) : IComparable<IdInformation>
{
    readonly int IComparable<IdInformation>.CompareTo(IdInformation other) => Value?.ToString()?.CompareTo(other.Value?.ToString()) ?? -1;

    /// <returns/>
    public readonly override string ToString() => $"{Type.FullName()} {Identifier} = {Value}";

    /// <returns>a snapshot optimized version of this data</returns>
    public readonly IdSnapshotInformation ToSnapshotInfo(bool includeValue = false)
        => new(Identifier, Value, includeValue);
}