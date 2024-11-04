namespace JLib.DataProvider;

public sealed class IgnoredEntity : IEntity
{
    private IgnoredEntity()
    {
    }

    public Guid Id { get; }

}
public sealed class IgnoredDataObject : IDataObject
{
    private IgnoredDataObject()
    {
    }

    public Guid Id { get; }
}