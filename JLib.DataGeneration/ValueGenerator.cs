using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using JLib.Helper;
using JLib.ValueTypes;
using ValueType = JLib.ValueTypes.ValueType;

namespace JLib.DataGeneration;

internal class ValueGenerator
{
    public object ToJsonObject()
        => new Dictionary<string, int>()
            {
                {nameof(_byte), _byte},
                {nameof(_sbyte), _sbyte},
                {nameof(_short), _short},
                {nameof(_ushort), _ushort},
                {nameof(_int), _int},
                {nameof(_uint), _uint},
                {nameof(_long), _long},
                {nameof(_ulong), _ulong},
                {nameof(_float), _float},
                {nameof(_double), _double},
                {nameof(_decimal), _decimal}
            }.Where(x => x.Value > 0)
            .ToDictionary(x => x.Key, x => x.Value);

    public static ValueGenerator FromJsonObject(JsonObject? obj)
    {
        var obj2 = obj.Deserialize<Dictionary<string, int>>();
        return obj2 is null
            ? new()
            : new()
            {
                _byte = obj2.GetValueOrDefault(nameof(_byte), 0),
                _sbyte = obj2.GetValueOrDefault(nameof(_sbyte), 0),
                _short = obj2.GetValueOrDefault(nameof(_short), 0),
                _ushort = obj2.GetValueOrDefault(nameof(_ushort), 0),
                _int = obj2.GetValueOrDefault(nameof(_int), 0),
                _uint = obj2.GetValueOrDefault(nameof(_uint), 0),
                _long = obj2.GetValueOrDefault(nameof(_long), 0),
                _ulong = obj2.GetValueOrDefault(nameof(_ulong), 0),
                _float = obj2.GetValueOrDefault(nameof(_float), 0),
                _double = obj2.GetValueOrDefault(nameof(_double), 0),
                _decimal = obj2.GetValueOrDefault(nameof(_decimal), 0)
            };
    }

    public TVt NextValueType<TVt, Tv>()
        where TVt : ValueType<Tv>
        where Tv : struct
#if NET6_0_OR_GREATER
        , INumber<Tv>
#endif
        => ValueType.Create<TVt, Tv>(Next<Tv>());

    public T Next<T>() where T : struct
#if NET6_0_OR_GREATER
        , INumber<T>
#endif
        => default(T) switch
        {
            byte => NextByte().CastTo<T>(),
            sbyte => NextSByte().CastTo<T>(),
            short => NextShort().CastTo<T>(),
            ushort => NextUShort().CastTo<T>(),
            int => NextInt().CastTo<T>(),
            uint => NextUInt().CastTo<T>(),
            long => NextLong().CastTo<T>(),
            ulong => NextULong().CastTo<T>(),
            float => NextFloat().CastTo<T>(),
            double => NextDouble().CastTo<T>(),
            decimal => NextDecimal().CastTo<T>(),
            _ => throw new ArgumentOutOfRangeException()
        };

    private int _byte;
    public byte NextByte() => Convert.ToByte(Interlocked.Increment(ref _byte));
    private int _sbyte;
    public sbyte NextSByte() => Convert.ToSByte(Interlocked.Increment(ref _sbyte));
    private int _short;
    public short NextShort() => Convert.ToInt16(Interlocked.Increment(ref _short));
    private int _ushort;
    public ushort NextUShort() => Convert.ToUInt16(Interlocked.Increment(ref _ushort));
    private int _int;
    public int NextInt() => Interlocked.Increment(ref _int);
    private int _uint;
    public uint NextUInt() => Convert.ToUInt32(Interlocked.Increment(ref _uint));
    private int _long;
    public long NextLong() => Interlocked.Increment(ref _long);
    private int _ulong;
    public ulong NextULong() => Convert.ToUInt32(Interlocked.Increment(ref _ulong));
    private int _float;
    public float NextFloat() => Interlocked.Increment(ref _float);
    private int _double;
    public double NextDouble() => Interlocked.Increment(ref _double);
    private int _decimal;
    public decimal NextDecimal() => Interlocked.Increment(ref _decimal);

}