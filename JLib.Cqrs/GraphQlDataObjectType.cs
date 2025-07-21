using JLib.DataProvider;
using JLib.DataProvider.AutoMapper;
using JLib.DataProvider.EfCore;
using JLib.Helper;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.Cqrs;

public abstract record GraphQlDataObjectType(Type Value) : DataObjectType(Value);

[ImplementsAny<IGraphQlDataObject>, NotAbstract, IsClass]
public record UnmappedGraphQlDataObjectType(Type Value) : GraphQlDataObjectType(Value);

[ImplementsAny<IMappedGraphQlDataObject<IEntity>>, NotAbstract, IsClass]
public record MappedGraphQlDataObjectType(Type Value) : GraphQlDataObjectType(Value)
{
    public ReadDataObjectType? ReadDataObject
        => Navigate(cache =>
            cache.TryGet<ReadDataObjectType>(Value.GetAnyInterface<IMappedGraphQlDataObject<IDataObject>>()));
}

[IsInterface, Implements<IDataObject>]
public record ReadDataObjectType(Type Value) : DataObjectType(Value)
{
    public ReadWriteEntityType ReadWriteEntity
        => Navigate(cache =>
            cache.All<ReadWriteEntityType>().Single(rwe => rwe.ReadOnlyEntity == this));

}

[IsClass, NotAbstract, Implements<IEntity>]
public record ReadWriteEntityType(Type Value) : EntityType(Value), IMappedDataObjectType
{
    public MappedGraphQlDataObjectType? GraphQlDataObject
        => Navigate(cache =>
            cache.All<MappedGraphQlDataObjectType>().FirstOrDefault(gdo => gdo.ReadDataObject == ReadOnlyEntity)
        );

    public ReadDataObjectType? ReadOnlyEntity
        => Navigate(cache =>
        {
            var roeType = Value
                .GetInterfaces()
                .Where(x => x.GetInterfaces().Contains(typeof(IEntity)))
                .ToArray();

            if (roeType.Length > 1)
                throw new InvalidOperationException($"Multiple interfaces found for {Value.Name}: {string.Join(", ", roeType.Select(t => t.FullName()))}");

            return cache.TryGet<ReadDataObjectType>(roeType.SingleOrDefault());
        });

    public ExplicitTypeMappingInfo[] MappingInfo
        => GraphQlDataObject is null ? [] :
        [
            new(GraphQlDataObject, this, MappingDataProviderMode.Read)
        ];
}

[AttributeUsage(AttributeTargets.Class)]
public class EfCoreEntityAttribute : Attribute { }

[IsClass, NotAbstract, Implements<IEntity>, HasAttribute(typeof(EfCoreEntityAttribute))]
public record ReadWriteEfCoreEntityType(Type Value) : ReadWriteEntityType(Value), IEfCoreEntityType;
