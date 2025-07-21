using JLib.Reflection;

namespace JLib.DataProvider;

/// <summary>
/// A Data Object Describes anything, which holds data. <see cref="EntityType"/> Describes an <see cref="DataObjectType"/> which directly accesses a data source like a database.
/// </summary>
/// <param name="Value"></param>
public abstract record DataObjectType(Type Value) : NavigatingTypeValueType(Value), IDataObjectType
{
    public const int NextPriority = 10_000;
}