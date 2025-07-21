using JLib.Reflection;

namespace JLib.DataProvider;

/// <summary>
/// enables a class to be requested and edited via <see cref="IDataProviderRw{TDataObject}"/> 
/// </summary>
[IgnoreInCache]
public interface IEntity : IDataObject
{
}