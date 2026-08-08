using System.Diagnostics;
using System.Security.Cryptography;

using JLib.Helper;
using JLib.ValueTypes.Implementations.FileSystem;

namespace JLib.ValueTypes.Implementations.Cryptography;

public static class FilePathHashExtensions
{
    private static readonly IReadOnlyCollection<Type> HashTargetInterfaces =
    [
        typeof(ISha1ValueType),
        typeof(ISha256ValueType),
        typeof(ISha384ValueType),
        typeof(ISha512ValueType),
    ];
    public static TOutputHash HashContent<TOutputHash>(this Stream stream)
        where TOutputHash : IHashValueType
    {
        var tOutputHash = typeof(TOutputHash);
        // create hash value
        byte[] hash;
        if (tOutputHash.IsAssignableTo<ISha1ValueType>())
            hash = SHA1.HashData(stream);
        else if (tOutputHash.IsAssignableTo<ISha256ValueType>())
            hash = SHA256.HashData(stream);
        else if (tOutputHash.IsAssignableTo<ISha384ValueType>())
            hash = SHA384.HashData(stream);
        else if (tOutputHash.IsAssignableTo<ISha512ValueType>())
            hash = SHA512.HashData(stream);
        else
            throw new ArgumentException(
                tOutputHash.FullName() + " does not implement any Interface to specify it's hash target. Valid Interfaces are ["
                                       + string.Join(", ", HashTargetInterfaces.Select(x => x.FullName())) + "]");


        if (tOutputHash.IsAssignableTo<BinaryHashes.BinaryHash>())
        {
            return ValueType.Create<TOutputHash,byte[]>(typeof(TOutputHash), hash);
        }
        else if (tOutputHash.IsAssignableTo<Base64Hashes.Base64Hash>())
        {

        }
        else if (tOutputHash.IsAssignableTo<HexadecimalHashes.HexadecimalHash>())
        {

        }
    }
}